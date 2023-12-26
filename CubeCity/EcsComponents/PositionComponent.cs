using CubeCity.Tools;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;

namespace CubeCity.EcsComponents;

public struct GenerateChunkRequestComponent
{
    public Vector2Int Position;
}

public struct PositionComponent : IEcsAutoReset<PositionComponent>
{
    public Vector3 Position;

    public void AutoReset(ref PositionComponent c)
    {
        Position = default;
    }
}
