using Microsoft.Xna.Framework.Graphics;

namespace CubeCity.Generators;

public readonly struct ChunkBindResponse
{
    public IndexBuffer Indices { get; }
    public VertexBuffer Vertices { get; }
    public ushort[,,] Blocks { get; }

    public ChunkBindResponse(IndexBuffer indices, VertexBuffer vertices, ushort[,,] blocks)
    {
        Indices = indices;
        Vertices = vertices;
        Blocks = blocks;
    }
}
