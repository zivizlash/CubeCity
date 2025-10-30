using CubeCity.Generators.Chunks;
using CubeCity.Generators.Models;
using CubeCity.Models;
using CubeCity.Pools;
using CubeCity.Services;
using CubeCity.Tools;
using Microsoft.Xna.Framework.Graphics;

namespace CubeCity.Generators.Pipelines;

public class ChunkGenerator(BlockType[] blockTypes, GraphicsDevice graphicsDevice,
    IChunkBlocksGenerator chunkGenerator)
{
    private Pooled<ushort[,,]> GenerateChunkBlocks(ChunkGenerateRequest request)
    {
        var pooledBlocks = ChunkBlocksPool.Get(16, 128);
        chunkGenerator.Generate(request.Position, pooledBlocks.Resource);
        return pooledBlocks;
    }

    public ChunkGenerateResponse GenerateChunkMesh(ChunkGenerateRequest request)
    {
        var blocks = GenerateChunkBlocks(request);
        var (indexBuffer, vertexBuffer) = CreateMeshBuffersByBlocks(blocks);
        var result = new ChunkGenerateResponseResult(new ChunkInfo(blocks), vertexBuffer, indexBuffer);
        return new ChunkGenerateResponse(request.Position, result);
    }

    public (IndexBuffer, VertexBuffer) CreateMeshBuffersByBlocks(Pooled<ushort[,,]> blocks)
    {
        var builder = new ChunkMeshGenerator(blockTypes, blocks.Resource);
        var pool = builder.Build();
        var mesh = pool.Items;

        var indexBuffer = new IndexBuffer(graphicsDevice,
            IndexElementSize.ThirtyTwoBits, mesh.TrianglesSize, BufferUsage.None);

        indexBuffer.SetData(mesh.InternalTriangles, 0, mesh.TrianglesSize);

        var vertexBuffer = new VertexBuffer(graphicsDevice,
            typeof(VertexPositionTexture), mesh.TextureSize, BufferUsage.None);

        vertexBuffer.SetData(mesh.InternalTexture, 0, mesh.TextureSize);

        pool.RemoveMemoryUser();

        return (indexBuffer, vertexBuffer);
    }
}
