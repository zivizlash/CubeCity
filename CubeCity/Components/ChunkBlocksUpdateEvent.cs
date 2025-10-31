using CubeCity.Tools;

namespace CubeCity.Components;

public struct ChunkBlocksUpdateEvent
{
    public Vector2Int ChunkPos;
    public Pooled<ushort[,,]> Blocks;
}
