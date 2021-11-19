using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace CubeCity.Tools
{
    [DebuggerDisplay("{X}:{Y}:{Z}")]
    public struct Vector3Int
    {
        public int X;
        public int Y;
        public int Z;

        public Vector3Int(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        
        public static Vector3Int operator +(Vector3Int left, Vector3Int right)
        {
            return new Vector3Int(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        public static Vector3Int operator -(Vector3Int left, Vector3Int right)
        {
            return new Vector3Int(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        public static Vector3 operator +(Vector3Int left, Vector3 right)
        {
            return new Vector3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }
    }
}
