using Leopotam.EcsLite;
using Microsoft.Xna.Framework;

namespace CubeCity.Components;

public struct PhysicsComponent : IEcsAutoReset<PhysicsComponent>
{
    public Vector3 Velocity;

    public void AutoReset(ref PhysicsComponent c)
    {
        c.Velocity = default;
    }
}
