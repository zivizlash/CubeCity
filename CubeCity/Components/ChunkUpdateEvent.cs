using CubeCity.Tools;

namespace CubeCity.Components;

public struct ChunkUpdateEvent
{
    public Vector2Int ChunkPos;
    public Pooled<ushort[,,]> Blocks;
}
