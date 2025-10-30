using CubeCity.Tools;

namespace CubeCity.Systems.Utils;

public interface IChunkIsRequiredChecker
{
    bool IsRequired(Vector2Int chunkPos);
}
