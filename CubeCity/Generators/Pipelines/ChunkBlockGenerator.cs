using CubeCity.Generators.Chunks;
using CubeCity.Generators.Models;
using CubeCity.Models;
using CubeCity.Systems.Utils;
using CubeCity.Tools;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

namespace CubeCity.Generators.Pipelines;

public class ChunkBlockGenerator
{
    private readonly ConcurrentQueue<ChunkGenerateResponse> _responses;
    private readonly ActionBlock<ChunkGenerateRequest> _requests;

    private readonly BlockType[] _blockTypes;
    private readonly GraphicsDevice _graphicsDevice;

    private readonly IChunkBlocksGenerator _chunkGenerator;
    private readonly IChunkIsRequiredChecker _chunkIsRequiredChecker;

    public ChunkBlockGenerator(BlockType[] blockTypes, 
        int generatingChunkThreads, GraphicsDevice graphicsDevice, 
        IChunkBlocksGenerator chunkGenerator, IChunkIsRequiredChecker chunkIsRequiredChecker)
    {
        _blockTypes = blockTypes;
        _graphicsDevice = graphicsDevice;
        _responses = new ConcurrentQueue<ChunkGenerateResponse>();

        _chunkGenerator = chunkGenerator;
        _chunkIsRequiredChecker = chunkIsRequiredChecker;

        _requests = new ActionBlock<ChunkGenerateRequest>(
            GenerateAndPostChunkMesh, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = generatingChunkThreads,
                SingleProducerConstrained = true
            });
    }

    public void AddGenerationRequest(ChunkGenerateRequest request)
    {
        if (!_requests.Post(request)) throw new InvalidOperationException();
    }

    public bool TryGetChunk(out ChunkGenerateResponse response)
    {
        return _responses.TryDequeue(out response);
    }

    private Pooled<ushort[,,]> GenerateChunkBlocks(ChunkGenerateRequest request)
    {
        var pooledBlocks = ChunkBlocksPool.Get(16, 128);
        _chunkGenerator.Generate(request.Position, pooledBlocks.Resource);
        return pooledBlocks;
    }

    public ChunkGenerateResponse GenerateChunkMesh(ChunkGenerateRequest request)
    {
        if (!_chunkIsRequiredChecker.IsRequired(request.Position))
        {
            return new ChunkGenerateResponse(request.Position, Result: null);
        }

        var blocks = GenerateChunkBlocks(request);
        var (indexBuffer, vertexBuffer) = CreateBuffers(blocks);
        var result = new ChunkGenerateResponseResult(new ChunkInfo(blocks), vertexBuffer, indexBuffer);
        return new ChunkGenerateResponse(request.Position, result);
    }

    private void GenerateAndPostChunkMesh(ChunkGenerateRequest request)
    {
        _responses.Enqueue(GenerateChunkMesh(request));
    }

    private (IndexBuffer, VertexBuffer) CreateBuffers(Pooled<ushort[,,]> blocks)
    {
        var builder = new ChunkMeshGenerator(_blockTypes, blocks.Resource);
        var pool = builder.Build();
        var mesh = pool.Items;

        var indexBuffer = new IndexBuffer(_graphicsDevice,
            IndexElementSize.ThirtyTwoBits, mesh.TrianglesSize, BufferUsage.None);

        indexBuffer.SetData(mesh.InternalTriangles, 0, mesh.TrianglesSize);

        var vertexBuffer = new VertexBuffer(_graphicsDevice,
            typeof(VertexPositionTexture), mesh.TextureSize, BufferUsage.None);

        vertexBuffer.SetData(mesh.InternalTexture, 0, mesh.TextureSize);

        pool.RemoveMemoryUser();

        return (indexBuffer, vertexBuffer);
    }
}
