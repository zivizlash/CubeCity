using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace CubeCity.Tools
{
    public class Vector2IntEqualityComparer : IEqualityComparer<Vector2Int>
    {
        public bool Equals(Vector2Int x, Vector2Int y)
        {
            return x.X == y.X && x.Y == y.Y;
        }

        public int GetHashCode(Vector2Int obj)
        {
            return obj.X << 16 | obj.Y;
        }
    }

    [DebuggerDisplay("{X}:{Y}")]
    public struct Vector2Int
    {
        public int X;
        public int Y;

        public static Vector2Int Zero => new();

        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Vector2Int operator +(Vector2Int left, Vector2Int right)
        {
            return new Vector2Int(left.X + right.X, left.Y + right.Y);
        }

        public static Vector2Int operator -(Vector2Int left, Vector2Int right)
        {
            return new Vector2Int(left.X - right.X, left.Y - right.Y);
        }

        public static bool operator ==(Vector2Int left, Vector2Int right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(Vector2Int left, Vector2Int right)
        {
            return left.X != right.X || left.Y != right.Y;
        }

        public static explicit operator Vector2Int(Vector3 position)
        {
            return new Vector2Int(
                (int)Math.Round(position.X, MidpointRounding.ToPositiveInfinity),
                (int)Math.Round(position.Z, MidpointRounding.ToPositiveInfinity));
        }

        public override string ToString()
        {
            return $"X: {X}; Y: {Y};";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}
