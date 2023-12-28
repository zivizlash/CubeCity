using Microsoft.Xna.Framework;

namespace CubeCity.GameObjects;

public class Camera
{
    private Vector3 _cameraForward;
    private Vector3 _cameraPosition;

    private Rectangle _clientBounds;

    public Vector3 Position => _cameraPosition;
    public Vector3 Forward => _cameraForward;

    public Matrix ViewMatrix => Matrix.CreateLookAt(_cameraPosition,
        _cameraPosition + _cameraForward, Vector3.Up);

    public Matrix ProjectionMatrix => Matrix.CreatePerspectiveFieldOfView(
        MathHelper.ToRadians(70), (float)_clientBounds.Width / _clientBounds.Height, 1, 1200);

    public Camera() : this(new Vector3(0, 64, 0))
    {
    }

    public Camera(Vector3 position)
    {
        _cameraPosition = position;
    }

    public void MoveTo(Vector3 position)
    {
        _cameraPosition = position;
    }

    public void MoveRelativelyBy(Vector3 offset)
    {
        var lookAtForward = Vector3.Normalize(new Vector3(_cameraForward.X, 0, _cameraForward.Z));
        var lookAtRight = Vector3.Normalize(Vector3.Cross(_cameraForward, Vector3.Up));

        _cameraPosition += lookAtRight * offset.X;
        _cameraPosition -= lookAtForward * offset.Z;
        _cameraPosition += Vector3.Up * offset.Y;
    }

    public void SetCameraForward(Vector3 forward)
    {
        _cameraForward = forward;
    }

    public void SetClientBounds(Rectangle bounds)
    {
        _clientBounds = bounds;
    }
} 
