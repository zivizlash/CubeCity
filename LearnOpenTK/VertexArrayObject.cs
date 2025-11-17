using OpenTK.Graphics.OpenGL4;

namespace LearnOpenTK;

public class VertexArrayObject : IDisposable
{
    private int Vao { get; }
    private int Vbo { get; }
    private int? Ebo { get; }
    private int Count { get; }

    public VertexArrayObject(float[] vertices, uint[]? indices)
    {
        Count = indices?.Length ?? vertices.Length / 5;

        Vao = GL.GenVertexArray();
        GL.BindVertexArray(Vao);

        Vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        if (indices is not null)
        {
            Ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ebo.Value);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
        }

        var stride = (3 + 2) * sizeof(float);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);
        
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        GL.BindVertexArray(0);
    }

    public void Draw()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        GL.BindVertexArray(Vao);

        if (Ebo is not null)
        {
            GL.DrawElements(PrimitiveType.Triangles, Count, DrawElementsType.UnsignedInt, 0);
        }
        else
        {
            GL.DrawArrays(PrimitiveType.Triangles, 0, Count);
        }
    }

    bool _disposed = false;
    
    ~VertexArrayObject()
    {
        if (!_disposed)
        {
            Console.WriteLine($"Memory leak for vao {Vao}");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(Vao);

            if (Ebo is not null)
            {
                GL.DeleteBuffer(Ebo.Value);
            }

            GL.DeleteBuffer(Vbo);
            GC.SuppressFinalize(this);
        }
    }
}
