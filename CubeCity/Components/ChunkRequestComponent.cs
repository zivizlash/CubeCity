using CubeCity.Tools;
using Leopotam.EcsLite;

namespace CubeCity.Components;

public struct ChunkRequestComponent : IEcsAutoReset<ChunkRequestComponent>
{
    public Vector2Int Position;

    public void AutoReset(ref ChunkRequestComponent c)
    {
        c.Position = default;
    }
}
