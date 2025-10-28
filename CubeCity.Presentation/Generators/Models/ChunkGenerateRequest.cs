using CubeCity.Tools;

namespace CubeCity.Generators.Models;

public readonly struct ChunkGenerateRequest
{
    public Vector2Int Position { get; }

    public ChunkGenerateRequest(Vector2Int position)
    {
        Position = position;
    }
}
