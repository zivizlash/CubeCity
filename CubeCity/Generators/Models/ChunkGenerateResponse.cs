using CubeCity.Models;
using CubeCity.Tools;
using Microsoft.Xna.Framework.Graphics;

namespace CubeCity.Generators.Models;

public readonly record struct ChunkGenerateResponseResult(
    ChunkInfo ChunkInfo,
    VertexBuffer VertexBuffer, 
    IndexBuffer IndexBuffer)
{
    public void Dispose()
    {
        VertexBuffer.Dispose();
        IndexBuffer.Dispose();
        ChunkInfo.Blocks.Dispose();
    }
}

public readonly record struct ChunkGenerateResponse(
    Vector2Int Position, 
    ChunkGenerateResponseResult? Result);
