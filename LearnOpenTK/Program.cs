using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LearnOpenTK;

public class VertexArrayObject : IDisposable
{
    private int Vao { get; }
    private int Vbo { get; }
    private int Ebo { get; }
    private int Count { get; }

    public VertexArrayObject(float[] vertices, uint[] indices)
    {
        Count = indices.Length;

        Vao = GL.GenVertexArray();
        GL.BindVertexArray(Vao);

        Vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        Ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        var stride = (3 + 3 + 2) * sizeof(float);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);
        
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);

        GL.BindVertexArray(0);
    }

    public void Draw()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        GL.BindVertexArray(Vao);
        GL.DrawElements(PrimitiveType.Triangles, Count, DrawElementsType.UnsignedInt, 0);
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
            GL.DeleteBuffer(Ebo);
            GL.DeleteBuffer(Vbo);
            GC.SuppressFinalize(this);
        }
    }
}

public class Game : GameWindow
{
    public Game(int width, int height, string title) : base(GameWindowSettings.Default, 
        new NativeWindowSettings { ClientSize = (width, height), Title = title })
    {
        _shader = new("shader4");

        float[] vertices = 
        [
            -0.5f, -0.5f, 0.0f, //Bottom-left vertex
            -0.5f,  0.5f, 0.0f, //Bottom-right vertex
             0.5f, -0.5f, 0.0f, //Top vertex
             0.5f,  0.5f, 0.0f  //Top vertex
        ];

        float[] verticesWithColor =
        [
             0.5f,  0.5f, 0.0f,   1.0f, 0.0f, 0.0f,   1.0f, 1.0f,   // Верхний правый
             0.5f, -0.5f, 0.0f,   0.0f, 1.0f, 0.0f,   1.0f, 0.0f,   // Нижний правый
            -0.5f, -0.5f, 0.0f,   0.0f, 0.0f, 1.0f,   0.0f, 0.0f,   // Нижний левый
            -0.5f,  0.5f, 0.0f,   1.0f, 1.0f, 0.0f,   0.0f, 1.0f    // Верхний левый
        ];
        
        uint[] indices =
        [
            2, 3, 1,
            1, 0, 3
        ];

        _vao = new(verticesWithColor, indices);
        _texture = new("texture1.png");
    }

    private readonly Texture2D _texture;
    private readonly Shader _shader;
    private readonly VertexArrayObject _vao;

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    private double _elapsed;

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            _vao.Dispose();
            Close();
        }

        _elapsed += args.Time;

        GL.ClearColor(0.0f, 0.0f, 0.0f, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        var transform = Matrix4.CreateRotationZ((float)_elapsed * 2.0f);
        transform *= Matrix4.CreateScale(0.5f, 0.5f, 0.5f);
        transform *= Matrix4.CreateTranslation(MathF.Sin((float)_elapsed) / 2, MathF.Cos((float)_elapsed) / 2, 0);

        _texture.Use();
        _shader.Use();

        // scale, rotation, translation

        GL.UniformMatrix4(_shader.GetUniform("transform"), false, ref transform);

        _vao.Draw();
        _texture.Unbind();

        SwapBuffers();
    }

    public override void Dispose()
    {
        _vao.Dispose();
        _shader.Dispose();
        _texture.Dispose();
        base.Dispose();
    }
}

internal class Program
{
    static void Main(string[] args)
    {
        Enumerable.Repeat("я люблю майнкрафт", 3).ToList().ForEach(Console.WriteLine);

        using var game = new Game(800, 600, "LearnOpenTK");
        game.Run();
    }
}
