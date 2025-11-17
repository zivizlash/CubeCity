using LearnOpenTK.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LearnOpenTK;

public class World
{
    private readonly List<IUpdatable> _updatables = [];
    private readonly List<IDrawable> _drawables = [];

    public void Update(float elapsed)
    {
        foreach (var updatable in _updatables)
        {
            updatable.Update(elapsed);
        }
    }

    public void Draw()
    {
        foreach (var drawable in _drawables)
        {
            drawable.Draw();
        }
    }

    public World Add(IComponent component)
    {
        if (component is IUpdatable updatable)
        {
            _updatables.Add(updatable);
        }

        if (component is IDrawable drawable)
        {
            _drawables.Add(drawable);
        }

        return this;
    }

    public World Add(IEnumerable<IComponent> components)
    {
        foreach (var component in components)
        {
            Add(component);
        }

        return this;
    }
}

public class WorldFactory
{
    public World Create(GameWindow gameWindow)
    {
        var camera = new Camera(gameWindow);

        var factory = new BoxFactory();
        var random = new Random(444);

        var world = new World();
        world.Add(camera);
        world.Add(Enumerable.Repeat(0, 10).Select(_ => factory.Create2(camera).RandomizePos(random)));

        return world;
    }
}

public class Game : GameWindow
{
    public Game(int width, int height, string title) : base(GameWindowSettings.Default, 
        new NativeWindowSettings { ClientSize = (width, height), Title = title })
    {
        _world = new WorldFactory().Create(this);
    }

    private readonly World _world;

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
        }

        GL.ClearColor(0.3f, 0.3f, 0.3f, 1);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _world.Update((float)args.Time);
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
