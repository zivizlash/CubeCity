using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace LearnOpenTK.Components;

public interface IComponent
{
}

public interface IDrawable : IComponent
{
    void Draw();
}

public interface IUpdatable : IComponent
{
    void Update(float elapsed);
}

public class Camera : IUpdatable
{
    public Matrix4 ProjectionViewMatrix { get; private set; }

    public Vector3 Position { get; set; }

    private readonly GameWindow _gameWindow;

    private float _total;

    public Camera(GameWindow gameWindow)
    {
        _gameWindow = gameWindow;
    }

    public void Update(float elapsed)
    {
        _total += elapsed;
        _total = 5f;
        UpdateInternal();
    }

    private void UpdateInternal()
    {
        var size = _gameWindow.Size;
        var aspectRation = (float)size.X / size.Y;
        var projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45), aspectRation, 0.1f, 100f);
        
        var pos = new Vector3(MathF.Sin(_total / 2) * 8, 12, MathF.Cos(_total / 2) * 8);
        var viewMatrix = Matrix4.LookAt(pos, Vector3.Zero, Vector3.UnitY);

        ProjectionViewMatrix = viewMatrix * projectionMatrix;
    }
}
