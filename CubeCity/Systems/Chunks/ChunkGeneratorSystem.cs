using CubeCity.Generators.Chunks;
using CubeCity.Pools;
using CubeCity.Threading;
using CubeCity.Tools;
using Leopotam.EcsLite;

namespace CubeCity.Systems.Chunks;

public struct ChunkBlocksUpdateEvent
{
    public Vector2Int ChunkPos;
    public Pooled<ushort[,,]> Blocks;
}

public struct ChunkBlocksFetchEvent
{
    public Vector2Int Pos;
}

public struct ChunkBlocksUnloadEvent
{
    public Vector2Int Pos;
}

public class ChunkBlockUpdatingInfo
{
    public required EcsPackedEntity PackedEntity;
}

public class ChunkGeneratorSystem : IEcsRunSystem
{
    private readonly IProcessorPipe<BlockGeneratorRequest, BlockGeneratorResponse> _pipe;
    private readonly IChunkBlocksGenerator _blocksGenerator;
    
    private readonly EcsWorld _world;

    private readonly EcsFilter _fetchEventsFilter;
    private readonly EcsPool<ChunkBlocksFetchEvent> _fetchEventsPool;
    private readonly EcsPool<ChunkBlocksUpdateEvent> _chunkBlocksFetchedPool;

    public ChunkGeneratorSystem(EcsWorld world, IChunkBlocksGenerator blocksGenerator,
        BackgroundManager backgroundManager)
    {
        _world = world;
        _blocksGenerator = blocksGenerator;

        _fetchEventsPool = world.GetPool<ChunkBlocksFetchEvent>();
        _fetchEventsFilter = world.Filter<ChunkBlocksFetchEvent>().End();
        _chunkBlocksFetchedPool = world.GetPool<ChunkBlocksUpdateEvent>();

        _pipe = backgroundManager.Create<BlockGeneratorRequest, BlockGeneratorResponse>(GenerateBlocks);
    }

    public void Run(IEcsSystems systems)
    {
        foreach (var entity in _fetchEventsFilter)
        {
            var chunkPos = _fetchEventsPool.Get(entity).Pos;
            _fetchEventsPool.Del(entity);
            _pipe.Enqueue(new BlockGeneratorRequest(chunkPos));
        }

        while (_pipe.TryPoll(out var result))
        {
            var entity = _world.NewEntity();
            ref var fetched = ref _chunkBlocksFetchedPool.Add(entity);
            fetched.ChunkPos = result.ChunkPos;
            fetched.Blocks = result.Blocks;
        }
    }

    private BlockGeneratorResponse GenerateBlocks(BlockGeneratorRequest request)
    {
        var pooledBlocks = ChunkBlocksPool.Get(16, 128);
        _blocksGenerator.Generate(request.ChunkPos, pooledBlocks.Resource);
        var response = new BlockGeneratorResponse(request.ChunkPos, pooledBlocks);
        return response;
    }

    public record BlockGeneratorRequest(Vector2Int ChunkPos);
    public record BlockGeneratorResponse(Vector2Int ChunkPos, Pooled<ushort[,,]> Blocks);
}
