using CubeCity.Tools;

namespace CubeCity.Services;

public interface IChunkIsRequiredChecker
{
    bool IsRequired(Vector2Int chunkPos);
}
