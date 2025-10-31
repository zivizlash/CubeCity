using CubeCity.Components;
using CubeCity.Generators.Pipelines;
using CubeCity.Models;
using CubeCity.Pools;
using CubeCity.Threading;
using CubeCity.Tools;
using Leopotam.EcsLite;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;

namespace CubeCity.Systems.Chunks;

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
    private readonly EcsPool<ChunkBlocksUpdateFlag> _chunkUpdateFlags;

    public ChunkMeshSystem(EcsWorld world, BackgroundManager backgroundManager, 
        GraphicsDevice graphicsDevice, BlockType[] blockTypes, ILogger<ChunkMeshSystem> logger)
    {
        _world = world;
        _graphicsDevice = graphicsDevice;
        _blockTypes = blockTypes;
        _logger = logger;
        _pipe = backgroundManager.Create<ChunkMeshRequest, ChunkMeshResponse>(GenerateMesh);
        _renderPool = world.GetPool<RenderComponent>();
        _chunkPool = world.GetPool<ChunkComponent>();
        _chunkUpdateFlags = world.GetPool<ChunkBlocksUpdateFlag>();
        _chunkUpdateFlagsFilter = world.Filter<ChunkBlocksUpdateFlag>().End();
    }

    public void Run(IEcsSystems systems)
    {
        EnqueueGenerateRequests();
        UpdateGeneratedMeshes();
    }

    private void UpdateGeneratedMeshes()
    {
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
                _logger.LogInformation("Error while unpacking chunk {ChunkPos}. " +
                    "Chunk may deleted while mesh generating", response.ChunkPos);
            }

            response.Mesh.RemoveMemoryUser();
        }
    }

    private void EnqueueGenerateRequests()
    {
        foreach (var entity in _chunkUpdateFlagsFilter)
        {
            ref var chunkUpdateFlag = ref _chunkUpdateFlags.Get(entity);
            ref var chunk = ref _chunkPool.Get(entity);

            var packedEntity = _world.PackEntity(entity);
            _pipe.Enqueue(new ChunkMeshRequest(chunk.Position, packedEntity, chunk.Blocks));

            _chunkUpdateFlags.Del(entity);
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

    private ChunkMeshResponse GenerateMesh(ChunkMeshRequest request)
    {
        var builder = new ChunkMeshGenerator(_blockTypes, request.Blocks.Resource);
        return new ChunkMeshResponse(request.ChunkPos, request.PackedEntity, builder.Build());
    }

    public record ChunkMeshRequest(Vector2Int ChunkPos, EcsPackedEntity PackedEntity, Pooled<ushort[,,]> Blocks);
    public record ChunkMeshResponse(Vector2Int ChunkPos, EcsPackedEntity PackedEntity, PooledMemory<TexturePositionVertices> Mesh);
}
