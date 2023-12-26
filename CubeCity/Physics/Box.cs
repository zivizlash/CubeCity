using System;
using System.Runtime.InteropServices;

namespace CubeCity.Physics;

public readonly struct Point
{
    public readonly float X;
    public readonly float Y;
    public readonly float Z;

    public Point(float x, float y, float z)
    {
        X = x; Y = y; Z = z;
    }
}

public readonly struct Vector2
{
    public readonly float X;
    public readonly float Y;

    public Vector2(float x, float y)
    {
        X = x; Y = y;
    }
}

public readonly struct Vector3
{
    public readonly float X;
    public readonly float Y;
    public readonly float Z;

    public Vector3(float x, float y, float z)
    {
        X = x; Y = y; Z = z;
    }
}

public struct Box
{
    public float MinX => Position.X - Bounds.X / 2.0f;
    public float MaxX => Position.X + Bounds.X / 2.0f;
    public float MinY => Position.Y - Bounds.Y / 2.0f;
    public float MaxY => Position.Y + Bounds.Y / 2.0f;
    public float MinZ => Position.Z - Bounds.Z / 2.0f;
    public float MaxZ => Position.Z + Bounds.Z / 2.0f;

    public Point Position { get; set; }
    public Vector3 Bounds { get; set; }

    public Box(Point position, Vector3 bounds)
    {
        Position = position; Bounds = bounds;    
    }
}

public static class BasePhysicsTools
{
    public static float Dot(this Vector3 a, Vector3 b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }

    public static float Distance(Point a, Point b)
    {
        return MathF.Sqrt(MathF.Pow(a.X - b.X, 2) + MathF.Pow(a.Y - b.Y, 2) + MathF.Pow(a.Z - b.Z, 2));
    }

    public static bool IsPointInside(Point point, Box box)
    {
        return point.X >= box.MinX
            && point.X <= box.MaxX
            && point.Y >= box.MinY
            && point.Y <= box.MaxY
            && point.Z >= box.MinZ
            && point.Z <= box.MaxZ;
    }

    public static bool Intersect(Box a, Box b)
    {
        return a.MinX <= b.MaxX
            && a.MaxX >= b.MinX
            && a.MinY <= b.MaxY
            && a.MaxY >= b.MinY
            && a.MinZ <= b.MaxZ
            && a.MaxZ >= b.MinZ;
    }
}
