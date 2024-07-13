using System;

namespace CubeCity.Physics.Engine;

public class PhysicsContainer
{
    private TimeSpan _accumulator;
    private TimeSpan _maxAccumulator = TimeSpan.FromSeconds(0.2f);

    public readonly PhysicsBody[] Bodies;
    public int TargetUpdateRate { get; set; }
    public double TargetOffset => 1.0 / TargetUpdateRate;
    public TimeSpan TargetSpan => TimeSpan.FromMilliseconds(TargetOffset);

    public PhysicsContainer()
    {
        TargetUpdateRate = 20;
        Bodies = new PhysicsBody[2];

        Bodies[0] = new PhysicsBody
        {
            Aabb = new Rect
            {
                Min = new Microsoft.Xna.Framework.Vector2(10, 10),
                Max = new Microsoft.Xna.Framework.Vector2(20, 20)
            },
            Restitution = 1,
            Velocity = new Microsoft.Xna.Framework.Vector2(0.5f, -0.5f)
        };
        Bodies[0].SetMass(10);

        Bodies[1] = new PhysicsBody
        {
            Aabb = new Rect
            {
                Min = new Microsoft.Xna.Framework.Vector2(40, 40),
                Max = new Microsoft.Xna.Framework.Vector2(50, 50)
            },
            Restitution = 1,
            Velocity = new Microsoft.Xna.Framework.Vector2(-0.5f, 0.5f)
        };
        Bodies[1].SetMass(20);
    }

    public void Update(TimeSpan dt)
    {
        _accumulator += dt;

        while (_accumulator > TargetSpan)
        {
            InternalUpdate(dt);
            _accumulator -= TargetSpan;
        }
    }

    private void InternalUpdate(TimeSpan dt)
    {

    }
}
