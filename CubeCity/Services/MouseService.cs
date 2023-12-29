using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace CubeCity.Services;

public class MouseService
{
    private readonly GameWindow _gameWindow;
    private readonly ILogger<MouseService> _logger;

    public bool IsCaptured { get; set; }
    public float MouseSensitivity { get; set; }
    public float GamepadSensitivity { get; set; }
    public MouseState State { get; private set; }

    private float _yaw = 90.0f;
    private float _pitch;

    private int _lastX, _lastY;

    private int _xOffset = 0, _yOffset = 0;

    public MouseService(GameWindow gameWindow, ILogger<MouseService> logger)
    {
        _gameWindow = gameWindow;
        _logger = logger;
    }

    public void UpdateState(MouseState state)
    {
        State = state;

        _xOffset = state.X - _lastX;
        _yOffset = _lastY - state.Y;

        _lastX = state.X;
        _lastY = state.Y;

        IsCaptured = state.MiddleButton == ButtonState.Pressed;
    }

    public Vector3 GetRotation(TimeSpan delta, float up, float right)
    {
        if (IsCaptured)
        {
            var mousePos = new Point(_lastX, _lastY);
            var size = _gameWindow.ClientBounds.Size;

            var x = size.X / 2;
            var y = size.Y / 2;

            if (IsOutOfScreen(new Point(x, y), mousePos, Math.Min(x, y)))
            {
                Mouse.SetPosition(x, y);
                _lastX = x;
                _lastY = y;
                _xOffset = 0;
                _yOffset = 0;
            }

            _yaw += _xOffset * MouseSensitivity;
            _pitch += _yOffset * MouseSensitivity;
        }
        else
        {
            // Обработка ввода контроллера.
            var force = GamepadSensitivity * delta.TotalMilliseconds * 2;

            _yaw += (float)(right * force);
            _pitch += (float)(up * force);
        }

        _pitch = Math.Clamp(_pitch, -89.9f, 89.9f);

        var pitchRad = MathHelper.ToRadians(_pitch);
        var yawRad = MathHelper.ToRadians(_yaw);

        var front = new Vector3
        {
            X = MathF.Cos(pitchRad) * MathF.Cos(yawRad),
            Y = MathF.Sin(pitchRad),
            Z = MathF.Cos(pitchRad) * MathF.Sin(yawRad)
        };

        return Vector3.Normalize(front);
    }

    private static bool IsOutOfScreen(Point center, Point position, int threshold)
    {
        var x = Math.Abs(position.X - center.X);
        var y = Math.Abs(position.Y - center.Y);

        return x >= threshold || y >= threshold;
    }
}
