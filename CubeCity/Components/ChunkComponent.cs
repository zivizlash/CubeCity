using CubeCity.Tools;
using Leopotam.EcsLite;

namespace CubeCity.Components;

public struct ChunkComponent : IEcsAutoReset<ChunkComponent>
{
    public Vector2Int Position;
    public Pooled<ushort[,,]> Blocks;

    public void AutoReset(ref ChunkComponent c)
    {
        c.Position = default;
        c.Blocks.Dispose();
    }
}
