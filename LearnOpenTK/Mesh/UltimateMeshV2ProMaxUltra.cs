using OpenTK.Graphics.OpenGL4;

namespace LearnOpenTK.Mesh;

public class UltimateMeshV2ProMaxUltra
{
    public required Vertex[] Vertices;
    public required uint[]? Indices;
    public required Texture[] Textures;

    private int _vao, _vbo, _ebo;

    public void Setup()
    {
        _vao = GL.GenVertexArray();
        GL.BindVertexArray(_vao);

        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * Vertex.Size, Vertices, BufferUsageHint.StaticDraw);

        if (Indices is not null)
        {
            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
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

    public void Draw()
    {
        GL.BindVertexArray(_vao);
/*
        int diffuseTextureNumber = 0;

        for (int i = 0; i < Textures.Length; i++)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + i);
            var texture = Textures[i];
            GL.Uniform1(shader.GetUniform("material.texture_diffuse" + diffuseTextureNumber++), i);
            GL.BindTexture(TextureTarget.Texture2D, texture.Id);
        }

        GL.ActiveTexture(TextureUnit.Texture0);
*/

        if (Indices is not null)
        {
            GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
        }
        else
        {
            GL.DrawArrays(PrimitiveType.Triangles, 0, Vertices.Length);
        }

        GL.BindVertexArray(0);
    }
}
