using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LearnOpenTK;

public class Test
{
    public void Test1(int width, int height)
    {
        var perspective = Matrix4.CreatePerspectiveFieldOfView(90, (float)width / height, 0.1f, 100f);
    }
}

public class BoxDrawable(VertexArrayObject vao, Shader shader, Texture2D? texture) : DrawableObject(vao, shader, texture)
{
    private float _totalTime;

    public override void Draw(float elapsed)
    {
        _totalTime += elapsed;

        var transform = Matrix4.CreateRotationZ(_totalTime * 2.0f);
        transform *= Matrix4.CreateScale(0.5f, 0.5f, 0.5f);
        transform *= Matrix4.CreateTranslation(MathF.Sin(_totalTime) / 2, MathF.Cos(_totalTime) / 2, 0);

        GL.UniformMatrix4(_shader.GetUniform("transform"), false, ref transform);

        DrawInternal();
    }
}

public class SimpleFactory
{
    public DrawableObject Create1()
    {
        var shader = new Shader("shader4");

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

        var vao = new VertexArrayObject(verticesWithColor, indices);
        var texture = new Texture2D("texture1.png");

        return new BoxDrawable(vao, shader, texture);
    }
}

public class Game : GameWindow
{
    public Game(int width, int height, string title) : base(GameWindowSettings.Default, 
        new NativeWindowSettings { ClientSize = (width, height), Title = title })
    {
        _dos = new()
        {
            new SimpleFactory().Create1()
        };
    }

    private List<DrawableObject> _dos;

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }

        GL.ClearColor(0.0f, 0.0f, 0.0f, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        foreach (var drawable in _dos)
        {
            drawable.Draw((float)args.Time);
        }

        SwapBuffers();
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
