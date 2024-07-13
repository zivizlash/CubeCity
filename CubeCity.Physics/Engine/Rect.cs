using Microsoft.Xna.Framework;
using System;

namespace CubeCity.Physics.Engine;

public struct Rect
{
    public Vector2 Min;
    public Vector2 Max;

    // min - нижняя граница по осям x и y

    public readonly Vector2 GetPosition()
    {
        return new Vector2((Min.X + Max.X) / 2f, (Min.Y + Max.Y) / 2f);
    }

    public void Move(Vector2 offset)
    {
        Min += offset;
        Max += offset;
    }
}

public struct Manifold
{
    public PhysicsBody A;
    public PhysicsBody B;
    public float Penetration;
    public Vector2 Normal;
}

public struct Circle
{
    public float Radius;
    public Vector2 Position;
}

public class PhysicsBody
{
    public float Restitution;
    public Vector2 Velocity;
    public Rect Aabb;
    public float Mass;

    public float InvMass;

    public void SetMass(float mass)
    {
        if (mass == 0)
        {
            Mass = 0;
            InvMass = 0;
        }
        else
        {
            Mass = mass;
            InvMass = 1 / mass;
        }
    }
}

public static class RectTools
{
    public static bool IsIntersect(Rect a, Rect b)
    {
        if (a.Max.X < b.Min.X || a.Min.X > b.Max.X) return false;
        if (a.Max.Y < b.Min.Y || a.Min.Y > b.Max.Y) return false;
        return true;
    }

    public static float Distance(Vector2 a, Vector2 b)
    {
        float x = a.X - b.X;
        float y = a.Y - b.Y;

        return MathF.Sqrt(x * x + y * y);
    }

    public static bool CircleVsCircle(ref Manifold m, Circle a, Circle b)
    {
        ref var bodyA = ref m.A;
        ref var bodyB = ref m.B;

        var normal = b.Position - a.Position;
        var r = a.Radius + b.Radius;
        r *= r;

        if (normal.LengthSquared() > r)
        {
            return false;
        }

        var distance = normal.Length();

        if (distance != 0)
        {
            m.Penetration = r - distance;
            m.Normal = normal / distance;
        }
        else // одинаковое положение кек
        {
            m.Penetration = a.Radius;
            m.Normal = new Vector2(1, 0);
        }

        return true;
    }

    public static bool BodyVsBody(ref Manifold m)
    {
        ref var a = ref m.A.Aabb;
        ref var b = ref m.B.Aabb;

        var normal = b.GetPosition() - a.GetPosition();

        var aExtentX = (a.Max.X - a.Min.X) / 2;
        var bExtentX = (b.Max.X - b.Min.Y) / 2;

        var overlapX = aExtentX + bExtentX - MathF.Abs(normal.X);

        if (overlapX > 0)
        {
            var aExtentY = (a.Max.Y - a.Min.Y) / 2;
            var bExtentY = (b.Max.Y - a.Min.Y) / 2;

            var overlapY = aExtentY + bExtentY - MathF.Abs(normal.Y);

            if (overlapY > 0)
            {
                if (overlapX > overlapY)
                {
                    if (normal.X < 0)
                    {
                        m.Normal = new Vector2(-1, 0);
                    }
                    else
                    {
                        m.Normal = new Vector2(1, 0);
                    }

                    m.Penetration = overlapX;
                    return true;
                }
                else
                {
                    if (normal.Y < 0)
                    {
                        m.Normal = new Vector2(0, -1);
                    }
                    else
                    {
                        m.Normal = new Vector2(0, 1);
                    }

                    m.Penetration = overlapY;
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsIntersect(Circle a, Circle b)
    {
        float r = a.Radius + b.Radius;
        float x = a.Position.X + b.Position.X;
        float y = a.Position.Y + b.Position.Y;

        return r * r < x * x + y * y;
    }

    public static float GetRestitution(PhysicsBody a, PhysicsBody b)
    {
        return MathF.Min(a.Restitution, b.Restitution);
    }

    public static Vector2 GetForce(float e, Vector2 normal, Vector2 from, Vector2 to)
    {
        return -e * (to - from) * normal;
    }

    public static float Dot(Vector2 a, Vector2 b)
    {
        return a.X * b.X + a.Y * b.Y;
    }

    public static Vector2 GetImpulse(float mass, Vector2 velocity)
    {
        return velocity * mass;
    }

    public static void ResolveCollision(PhysicsBody a, PhysicsBody b, Vector2 normal)
    {
        var rv = b.Velocity - a.Velocity;

        float velocityAlongNormal = Dot(rv, normal);

        if (velocityAlongNormal > 0) return;

        var e = GetRestitution(a, b);

        float j = -(1 + e) * velocityAlongNormal;

        j /= (1 / a.Mass) + (1 / b.Mass);

        var impulse = j * normal;

        float massSum = a.Mass + b.Mass;

        a.Velocity -= a.Mass / massSum * impulse;
        b.Velocity += b.Mass / massSum * impulse;
    }

    public static void PositionalCorrection(PhysicsBody a, PhysicsBody b, Vector2 penetrationDepth, Vector2 normal)
    {
        const float percent = 0.2f;

        var correction = penetrationDepth / (a.InvMass + b.InvMass) * percent * normal;

        //a.Posi
    }
}
