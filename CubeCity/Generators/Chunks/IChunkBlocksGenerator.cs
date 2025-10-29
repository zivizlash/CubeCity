using CubeCity.Tools;

namespace CubeCity.Generators.Chunks;

public interface IChunkBlocksGenerator
{
    void Generate(Vector2Int chunkPos, ushort[,,] blocks);
}
