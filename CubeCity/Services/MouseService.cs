using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Input;

namespace CubeCity.Services;

public class MouseService
{
    private readonly GameWindow _gameWindow;

    public bool IsCaptured { get; set; }
    public float MouseSensitivity { get; set; }
    public float GamepadSensitivity { get; set; }

    private float _yaw = 90.0f;
    private float _pitch;

    private int _lastX, _lastY;

    public MouseService(GameWindow gameWindow)
    {
        _gameWindow = gameWindow;
    }

    private static bool IsOutOfScreen(Point center, Point position, int threshold)
    {
        var x = Math.Abs(center.X - position.X);
        var y = Math.Abs(center.Y - position.Y);

        return x > threshold || y > threshold;
    }

    public Vector3 GetRotation(Point mousePosition, TimeSpan delta, float up, float right)
    {
        int xOffset = mousePosition.X - _lastX;
        int yOffset = _lastY - mousePosition.Y;

        _lastX = mousePosition.X;
        _lastY = mousePosition.Y;

        if (IsCaptured)
        {
            var center = _gameWindow.ClientBounds.Center;
            var threshold = Math.Min(_gameWindow.ClientBounds.Width, _gameWindow.ClientBounds.Height) / 4;

            if (IsOutOfScreen(center, mousePosition, threshold))
            {
                Mouse.SetPosition(center.X, center.Y);
                _lastX = center.X;
                _lastY = center.Y;
            }

            _yaw += xOffset * MouseSensitivity;
            _pitch += yOffset * MouseSensitivity;
        }
        else
        {
            // Обработка ввода контроллера.
            var mult = GamepadSensitivity * delta.TotalMilliseconds * 2;

            _yaw += (float)(right * mult);
            _pitch += (float)(up * mult);
        }

        if (_pitch > 89.0f)
            _pitch = 89.0f;

        if (_pitch < -89.0f)
            _pitch = -89.0f;

        Vector3 front = default;

        front.X = MathF.Cos(MathHelper.ToRadians(_pitch)) * MathF.Cos(MathHelper.ToRadians(_yaw));
        front.Y = MathF.Sin(MathHelper.ToRadians(_pitch));
        front.Z = MathF.Cos(MathHelper.ToRadians(_pitch)) * MathF.Sin(MathHelper.ToRadians(_yaw));

        return Vector3.Normalize(front);
    }
}
