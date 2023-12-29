using CubeCity.Tools;
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
