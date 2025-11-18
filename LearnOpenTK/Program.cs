using LearnOpenTK.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LearnOpenTK;

public class Game : GameWindow
{
    public Game(int width, int height, string title) : base(GameWindowSettings.Default, 
        new NativeWindowSettings { ClientSize = (width, height), Title = title })
    {
        _shaders = new Shaders();
        (_world, _camera) = new WorldFactory().Create(this, _shaders.Basic);
    }

    private readonly Shaders _shaders;
    private readonly World _world;
    private readonly Camera _camera;

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.Enable(EnableCap.DepthTest);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
            return;
        }

        GL.ClearColor(0.3f, 0.3f, 0.44f, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _world.Update((float)args.Time);

        _shaders.Basic.Use();
        _shaders.Basic.Transform.SetValue(_camera.ProjectionViewMatrix);
        _shaders.Lightsource.Use();
        _shaders.Lightsource.Transform.SetValue(_camera.ProjectionViewMatrix);

        _world.Draw();

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
