using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp;

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

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindVertexArray(0);
    }

    public void Draw()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        GL.BindVertexArray(Vao);
        GL.DrawElements(PrimitiveType.Triangles, Count, DrawElementsType.UnsignedInt, 0);
    }

    bool _disposed = false;

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

public class Shader : IDisposable
{
    public string Name { get; }

    private readonly int _vertShader;
    private readonly int _fragShader;
    private readonly int _handle;

    private readonly Dictionary<string, int> _nameToLocation = new();

    public Shader(string name)
    {
        Name = name;

        var vertSource = File.ReadAllText($"{name}.vert");
        var fragSource = File.ReadAllText($"{name}.frag");

        _vertShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(_vertShader, vertSource);
        CompileShader(_vertShader);

        _fragShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(_fragShader, fragSource);
        CompileShader(_fragShader);

        _handle = GL.CreateProgram();

        GL.AttachShader(_handle, _vertShader);
        GL.AttachShader(_handle, _fragShader);

        GL.LinkProgram(_handle);

        GL.DeleteShader(_vertShader);
        GL.DeleteShader(_fragShader);

        GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out var success);

        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(_handle);
            throw new Exception($"Error while linking program: {infoLog}");
        }
    }

    private static void CompileShader(int shader)
    {
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);

        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(shader);
            throw new Exception($"Shader compilation error: {infoLog}");
        }
    }

    public int GetAttribLocation(string attribName)
    {
        return GL.GetAttribLocation(_handle, attribName);
    }

    public int GetUniform(string name)
    {
        if (_nameToLocation.TryGetValue(name, out var location))
        {
            return location;
        }

        location = GL.GetUniformLocation(_handle, name);

        if (location == -1)
        {
            throw new Exception($"Uniform {name} location not found");
        }

        _nameToLocation[name] = location;
        return location;
    }

    public void Use()
    {
        CheckDisposed();
        GL.UseProgram(_handle);
    }

    private bool _disposed = false;

    private void CheckDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(Shader), $"Shader with name {Name} disposed");
        }
    }

    ~Shader()
    {
        if (!_disposed)
        {
            Console.WriteLine($"GPU resource leak for shader {Name}");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            GL.DeleteProgram(_handle);
            GC.SuppressFinalize(this);
        }
    }
}

public class Game : GameWindow
{
    public Game(int width, int height, string title) : base(GameWindowSettings.Default, 
        new NativeWindowSettings { ClientSize = (width, height), Title = title })
    {
        _shader = new("shader2");

        float[] vertices = 
        [
            -0.5f, -0.5f, 0.0f, //Bottom-left vertex
            -0.5f,  0.5f, 0.0f, //Bottom-right vertex
             0.5f, -0.5f, 0.0f, //Top vertex
             0.5f,  0.5f, 0.0f  //Top vertex
        ];

        float[] verticesWithColor =
        [
            -0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, //Bottom-left vertex
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f, 0.0f, //Bottom-right vertex
             0.5f, -0.5f, 0.0f, 0.0f, 0.0f, 1.0f, //Top vertex
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, 1.0f, //Top vertex
        ];
        
        float[] texCoords = [
            0.0f, 0.0f,  // Нижний левый угол 
            1.0f, 0.0f,  // Нижний правый угол
            0.5f, 1.0f   // Верхняя центральная сторона
        ];

        uint[] indices =
        [
            0, 1, 2,
            2, 1, 3
        ];

        _vao = new(vertices, indices);
    }

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

        _shader.Use();

        var location = _shader.GetUniform("ourColor");
        var greenColor = Math.Sin(_elapsed);
        GL.Uniform4(location, 0.0f, (float)greenColor, 0.0f, 1.0f);
        _vao.Draw();

        SwapBuffers();
    }
}

internal class Program
{
    static void Main(string[] args)
    {
        using var game = new Game(800, 600, "LearnOpenTK");
        game.Run();
    }
}
