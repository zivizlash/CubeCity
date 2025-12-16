using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace LearnOpenTK;

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

public struct Texture
{
    public uint Id;
    public string Type;
}

public class UltimateMeshV2ProMaxUltra
{
    public required Vertex[] Vertices;
    public required uint[] Indices;
    public required Texture[] Textures;

    private int Vao, Vbo, Ebo;

    public void Setup()
    {
        Vao = GL.GenVertexArray();
        GL.BindVertexArray(Vao);

        Vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * Vertex.Size, Vertices, BufferUsageHint.StaticDraw);

        if (Indices is not null)
        {
            Ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices, BufferUsageHint.StaticDraw);
        }

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.Size, Vertex.PositionOffset);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Vertex.Size, Vertex.TexCoordsOffset);
        GL.EnableVertexAttribArray(1);

        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Vertex.Size, Vertex.NormalOffset);
        GL.EnableVertexAttribArray(2);

        GL.BindVertexArray(0);
    }

    public void Draw(Shader shader)
    {
        GL.BindVertexArray(Vao);

        if (Ebo != 0)
        {
            GL.DrawArrays(PrimitiveType.Triangles, 0, Vertices.Length);
        }
        else
        {
            GL.DrawArrays(PrimitiveType.Triangles, 0, Vertices.Length);
        }
    }
}

public class Shaders
{
    public readonly BasicShader Lightsource;
    public readonly BasicShader Basic;

    public static readonly Vector3 LightPos = new(0, 7, 0);

    public Shaders()
    {
        Lightsource = new BasicShader("light_source");
        Basic = new BasicShader("shader5");
    }
}
