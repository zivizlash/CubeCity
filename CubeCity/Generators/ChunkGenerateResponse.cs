using CubeCity.Models;
using CubeCity.Tools;
using Microsoft.Xna.Framework.Graphics;

namespace CubeCity.Generators
{
    public readonly struct ChunkGenerateResponse
    {
        public ChunkInfo ChunkInfo { get; }
        public Vector2Int Position { get; }
        public VertexBuffer VertexBuffer { get; }
        public IndexBuffer IndexBuffer { get; }

        public ChunkGenerateResponse(ChunkInfo chunkInfo, Vector2Int position, 
            VertexBuffer vertexBuffer, IndexBuffer indexBuffer)
        {
            ChunkInfo = chunkInfo;
            Position = position;
            VertexBuffer = vertexBuffer;
            IndexBuffer = indexBuffer;
        }
    }
}
