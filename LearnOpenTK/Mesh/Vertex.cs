using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace LearnOpenTK.Mesh;

[StructLayout(LayoutKind.Explicit)]
public struct Vertex
{
    public const int PositionOffset = sizeof(float) * 0;
    public const int TexCoordsOffset = sizeof(float) * 3;
    public const int NormalOffset = sizeof(float) * 5;
    public const int Size = sizeof(float) * 8;

    [FieldOffset(PositionOffset)]
    public Vector3 Position;

    [FieldOffset(TexCoordsOffset)]
    public Vector2 TexCoords;

    [FieldOffset(NormalOffset)]
    public Vector3 Normal;
}
