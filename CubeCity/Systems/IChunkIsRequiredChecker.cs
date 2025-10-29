using CubeCity.Tools;

namespace CubeCity.Systems;

public interface IChunkIsRequiredChecker
{
    bool IsRequired(Vector2Int chunkPos);
}
