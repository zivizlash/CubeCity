using Leopotam.EcsLite;
using Microsoft.Xna.Framework;

namespace CubeCity.EcsComponents;

public struct PositionComponent : IEcsAutoReset<PositionComponent>
{
    public Vector3 Position;

    public void AutoReset(ref PositionComponent c)
    {
        c.Position = default;
    }
}
