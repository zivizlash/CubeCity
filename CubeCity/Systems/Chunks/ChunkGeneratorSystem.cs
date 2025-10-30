using CubeCity.Components;
using CubeCity.Generators.Chunks;
using CubeCity.Generators.Models;
using CubeCity.Generators.Pipelines;
using CubeCity.Models;
using CubeCity.Pools;
using CubeCity.Systems.Chunks.Models;
using CubeCity.Threading;
using CubeCity.Tools;
using Leopotam.EcsLite;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

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

public class ChunkMeshInfo
{
    public EcsPackedEntity? PackedEntity;
}

public struct ChunkUpdateFlag
{
}

public class ChunkMeshSystem : IEcsRunSystem
{
    private readonly EcsWorld _world;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly BlockType[] _blockTypes;
    private readonly ILogger<ChunkMeshSystem> _logger;
    private readonly IProcessorPipe<ChunkMeshRequest, ChunkMeshResponse> _pipe;
    
    private readonly EcsFilter _chunkUpdateFlagsFilter;

    private readonly EcsPool<RenderComponent> _renderPool;
    private readonly EcsPool<ChunkComponent> _chunkPool;
    private readonly EcsPool<ChunkUpdateFlag> _chunkUpdateFlags;

    public ChunkMeshSystem(EcsWorld world, BackgroundManager backgroundManager, 
        GraphicsDevice graphicsDevice, BlockType[] blockTypes, ILogger<ChunkMeshSystem> logger)
    {
        _world = world;
        _graphicsDevice = graphicsDevice;
        _blockTypes = blockTypes;
        _logger = logger;
        _pipe = backgroundManager.Create<ChunkMeshRequest, ChunkMeshResponse>(MeshGenerate);
        _renderPool = world.GetPool<RenderComponent>();
        _chunkPool = world.GetPool<ChunkComponent>();
        _chunkUpdateFlags = world.GetPool<ChunkUpdateFlag>();
        _chunkUpdateFlagsFilter = world.Filter<ChunkUpdateFlag>().End();
    }

    public void Run(IEcsSystems systems)
    {
        foreach (var entity in _chunkUpdateFlagsFilter)
        {
            ref var chunkUpdateFlag = ref _chunkUpdateFlags.Get(entity);
            ref var chunk = ref _chunkPool.Get(entity);

            var packedEntity = _world.PackEntity(entity);
            _pipe.Enqueue(new ChunkMeshRequest(chunk.Position, packedEntity, chunk.Blocks));

            _chunkUpdateFlags.Del(entity);
        }

        while (_pipe.TryPoll(out var response))
        {
            if (response.PackedEntity.Unpack(_world, out var entity))
            {
                if (_renderPool.Has(entity))
                {
                    ref var render = ref _renderPool.Get(entity);
                    render.VertexBuffer.Dispose();
                    render.IndexBuffer.Dispose();
                    SetMeshData(response.Mesh.Items, ref render);
                }
                else
                {
                    ref var render = ref _renderPool.Add(entity);
                    SetMeshData(response.Mesh.Items, ref render);
                }
            }
            else
            {
                _logger.LogError("Error while unpacking chunk {ChunkPos}", response.ChunkPos);
            }

            response.Mesh.RemoveMemoryUser();
        }
    }

    private void SetMeshData(TexturePositionVertices mesh, ref RenderComponent render)
    {
        var indexBuffer = new IndexBuffer(_graphicsDevice,
            IndexElementSize.ThirtyTwoBits, mesh.TrianglesSize, BufferUsage.None);

        indexBuffer.SetData(mesh.InternalTriangles, 0, mesh.TrianglesSize);

        var vertexBuffer = new VertexBuffer(_graphicsDevice,
            typeof(VertexPositionTexture), mesh.TextureSize, BufferUsage.None);

        vertexBuffer.SetData(mesh.InternalTexture, 0, mesh.TextureSize);

        render.VertexBuffer = vertexBuffer;
        render.IndexBuffer = indexBuffer;
    }

    private ChunkMeshResponse MeshGenerate(ChunkMeshRequest request)
    {
        var builder = new ChunkMeshGenerator(_blockTypes, request.Blocks.Resource);
        return new ChunkMeshResponse(request.ChunkPos, request.PackedEntity, builder.Build());
    }

    public record ChunkMeshRequest(Vector2Int ChunkPos, EcsPackedEntity PackedEntity, Pooled<ushort[,,]> Blocks);
    public record ChunkMeshResponse(Vector2Int ChunkPos, EcsPackedEntity PackedEntity, PooledMemory<TexturePositionVertices> Mesh);
}

public class ChunkBlockUpdatingInfo
{
    public required EcsPackedEntity PackedEntity;
}

public class ChunkBlockUpdatingSystem(EcsWorld world) : IEcsRunSystem
{
    private readonly Dictionary<Vector2Int, ChunkBlockUpdatingInfo> _chunkPosToInfo = new(512);
    private readonly EcsPool<ChunkBlocksUpdateEvent> _blocksUpdatesEvents = world.GetPool<ChunkBlocksUpdateEvent>();
    private readonly EcsPool<PositionComponent> _positionPool = world.GetPool<PositionComponent>();
    private readonly EcsPool<ChunkComponent> _chunkPool = world.GetPool<ChunkComponent>();
    private readonly EcsPool<ChunkUpdateFlag> _chunkUpdateFlags = world.GetPool<ChunkUpdateFlag>();
    private readonly EcsFilter _chunkBlockUpdatesFilter = world.Filter<ChunkBlocksUpdateEvent>().End();

    public void Run(IEcsSystems systems)
    {
        foreach (var updateEntity in _chunkBlockUpdatesFilter)
        {
            ref var chunkBlockUpdate = ref _blocksUpdatesEvents.Get(updateEntity);

            if (_chunkPosToInfo.TryGetValue(chunkBlockUpdate.ChunkPos, out var chunkInfo))
            {
                var entity = chunkInfo.PackedEntity.Unpack(world);

                // todo: это пипяу, по факту он может участвовать в генерации меша
                // и мне нужно использовать нормальные пулы с подсчетом ссылок 
                // аааааааааааааааааааааааааааааааааааааааааааа
                ref var chunk = ref _chunkPool.Get(entity);
                chunk.Blocks.Dispose();
                chunk.Blocks = chunkBlockUpdate.Blocks;

                if (!_chunkUpdateFlags.Has(entity))
                {
                    _chunkUpdateFlags.Add(entity);
                }
            }
            else
            {
                var entity = world.NewEntity();
                var packedEntity = world.PackEntity(entity);

                chunkInfo = new ChunkBlockUpdatingInfo { PackedEntity = packedEntity };
                _chunkPosToInfo.Add(chunkBlockUpdate.ChunkPos, chunkInfo);

                ref var chunk = ref _chunkPool.Add(entity);
                chunk.Blocks = chunkBlockUpdate.Blocks;
                chunk.Position = chunkBlockUpdate.ChunkPos;

                ref var position = ref _positionPool.Add(entity);
                position.Position = new Vector3(
                    chunkBlockUpdate.ChunkPos.X * 16,
                    0,
                    chunkBlockUpdate.ChunkPos.Y * 16);

                _chunkUpdateFlags.Add(entity);
            }

            _blocksUpdatesEvents.Del(updateEntity);
        }
    }
}

public class ChunkBlockGeneratorSystem : IEcsRunSystem
{
    private readonly IProcessorPipe<BlockGeneratorRequest, BlockGeneratorResponse> _pipe;
    private readonly IChunkBlocksGenerator _blocksGenerator;
    
    private readonly EcsWorld _world;

    private readonly EcsFilter _fetchEventsFilter;
    private readonly EcsPool<ChunkBlocksFetchEvent> _fetchEventsPool;
    private readonly EcsPool<ChunkBlocksUpdateEvent> _chunkBlocksFetchedPool;

    public ChunkBlockGeneratorSystem(EcsWorld world, IChunkBlocksGenerator blocksGenerator,
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

public class ChunkGeneratorSystem : IEcsRunSystem
{
    private readonly IProcessorPipe<GeneratorRequest, GeneratorResponse> _pipe;
    private readonly ILogger<ChunkGeneratorSystem> _logger;
    private readonly ChunkGenerator _chunkGenerator;
    private readonly Dictionary<Vector2Int, ChunkLoaderInfo> _chunkPosToInfo;

    private readonly EcsWorld _world;

    private readonly EcsPool<ChunkRequestLoadEvent> _loadEvents;
    private readonly EcsPool<ChunkRequestUnloadEvent> _unloadPool;
    private readonly EcsFilter _loadFilter;
    private readonly EcsFilter _unloadFilter;

    private readonly EcsPool<ChunkComponent> _chunksPool;
    private readonly EcsPool<RenderComponent> _renderPool;
    private readonly EcsPool<PositionComponent> _positionPool;

    public ChunkGeneratorSystem(EcsWorld world, ChunkGenerator chunkGenerator,
        BackgroundManager backgroundManager, ILogger<ChunkGeneratorSystem> logger)
    {
        _pipe = backgroundManager.Create<GeneratorRequest, GeneratorResponse>(CreateChunk);
        _chunkGenerator = chunkGenerator;
        _chunkPosToInfo = new(64);
        _logger = logger;

        _world = world;
        _loadEvents = world.GetPool<ChunkRequestLoadEvent>();
        _unloadPool = world.GetPool<ChunkRequestUnloadEvent>();
        _loadFilter = world.Filter<ChunkRequestLoadEvent>().End();
        _unloadFilter = world.Filter<ChunkRequestUnloadEvent>().End();

        _chunksPool = world.GetPool<ChunkComponent>();
        _renderPool = world.GetPool<RenderComponent>();
        _positionPool = world.GetPool<PositionComponent>();
    }

    public void Run(IEcsSystems systems)
    {
        while (_pipe.TryPoll(out var generatorResult))
        {
            PlaceGeneratedInWorld(generatorResult);
        }
        EnqueueGeneratingRequests();
        UnloadUnusedChunks();
    }

    private void PlaceGeneratedInWorld(GeneratorResponse response)
    {
        var chunkInfo = _chunkPosToInfo[response.ChunkPos];
        var result = response.Result;

        if (chunkInfo.State is ChunkLoaderState.Loaded)
        {
            _logger.LogInformation("Skipped chunk update due state {State}", chunkInfo.State);
            // next step updating chunk when blocks updated
            result.Dispose();
            return;
        }

        var entity = _world.NewEntity();
        chunkInfo.PackedEntity = _world.PackEntity(entity);

        ref var chunk = ref _chunksPool.Add(entity);
        chunk.Blocks = result.ChunkInfo.Blocks;
        chunk.Position = response.ChunkPos;

        ref var render = ref _renderPool.Add(entity);
        render.VertexBuffer = result.VertexBuffer;
        render.IndexBuffer = result.IndexBuffer;

        ref var position = ref _positionPool.Add(entity);
        position.Position = new Vector3(
            response.ChunkPos.X * 16,
            0,
            response.ChunkPos.Y * 16);

        chunkInfo.State = ChunkLoaderState.Loaded;
    }

    private void EnqueueGeneratingRequests()
    {
        foreach (var entity in _loadFilter)
        {
            var chunkPos = _loadEvents.Get(entity).Pos;
            _loadEvents.Del(entity);

            if (_chunkPosToInfo.TryGetValue(chunkPos, out var chunkInfo))
            {
                _logger.LogError(
                    "Got LoadComponent while entry already exists. State: {State}",
                    chunkInfo.State);
                continue;
            }

            _chunkPosToInfo.Add(chunkPos, new ChunkLoaderInfo(ChunkLoaderState.Generating));
            _pipe.Enqueue(new GeneratorRequest(chunkPos));
        }
    }

    private void UnloadUnusedChunks()
    {
        foreach (var unloadEntity in _unloadFilter)
        {
            var chunkPos = _unloadPool.Get(unloadEntity).Pos;
            _unloadPool.Del(unloadEntity);

            if (!_chunkPosToInfo.TryGetValue(chunkPos, out var chunkInfo))
            {
                _logger.LogInformation(
                    "Internal entry not found for unloading chunk: {ChunkPos}", 
                    chunkPos);
                continue;
            }

            _chunkPosToInfo.Remove(chunkPos);

            if (chunkInfo.State == ChunkLoaderState.Generating)
            {
                _logger.LogInformation(
                    "Unloaded ungenerated chunk: {ChunkPos}", 
                    chunkPos);
                continue;
            }

            if (chunkInfo.State == ChunkLoaderState.Loaded)
            {
                var entity = chunkInfo.PackedEntity!.Value.Unpack(_world);
                _world.DelEntity(entity);
            }
            else
            {
                _logger.LogError("Expected Loaded state, but got {State}: {ChunkPos}", 
                    chunkInfo.State, chunkPos);
                continue;
            }
        }
    }

    private GeneratorResponse CreateChunk(GeneratorRequest request)
    {
        var chunkMesh = _chunkGenerator.GenerateChunkMesh(new ChunkGenerateRequest(request.Pos));
        return new GeneratorResponse(chunkMesh.Result!.Value, chunkMesh.Position);
    }

    public record GeneratorRequest(Vector2Int Pos);
    public record GeneratorResponse(ChunkGenerateResponseResult Result, Vector2Int ChunkPos);
}
