using CubeCity.GameObjects;
using CubeCity.Input;
using CubeCity.Services;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace CubeCity.Systems;

public class CameraSystem : IEcsRunSystem
{
    private readonly GamepadInputManager _gamepadManager;
    private readonly KeyboardInputManager _keyboardManager;
    private readonly MouseService _mouseManager;
    private readonly Camera _camera;
    private readonly GameWindow _gameWindow;
    private readonly ITime _time;

    private const float _controllerThreshold = 0.001f;
    private const float _moveThreshold = 0.001f;
    private const float _moveSpeed = 0.5f;

    public CameraSystem(GamepadInputManager gamepadManager, KeyboardInputManager keyboardManager, 
        MouseService mouseManager, Camera camera, GameWindow gameWindow, ITime time)
    {
        _gamepadManager = gamepadManager;
        _keyboardManager = keyboardManager;
        _mouseManager = mouseManager;
        _camera = camera;
        _gameWindow = gameWindow;
        _time = time;
    }

    public void Run(IEcsSystems systems)
    {
        UpdateCamera();

        var (gamepadAccelerate, gamepadTranslation) = GetGamepadTranslation(_gamepadManager);
        var (keyboardAccelerate, keyboardTranslation) = GetKeyboardTranslation(_keyboardManager);

        var translation = gamepadTranslation + keyboardTranslation;
        var accelerate = gamepadAccelerate + keyboardAccelerate;

        MoveCamera(_camera, translation, accelerate, _time.ElapsedTime);
    }

    private void UpdateCamera()
    {
        var sticks = _gamepadManager.State.ThumbSticks;

        var rotation = _mouseManager.GetRotation(_time.ElapsedTime, sticks.Right.Y, sticks.Right.X);

        _camera.SetClientBounds(_gameWindow.ClientBounds);
        _camera.SetCameraForward(rotation);
    }

    private static (float, Vector3) GetKeyboardTranslation(KeyboardInputManager keyboard)
    {
        var translation = Vector3.Zero;

        if (keyboard.IsKeyDown(Keys.A))
            translation += Vector3.Left;

        if (keyboard.IsKeyDown(Keys.D))
            translation += Vector3.Right;

        if (keyboard.IsKeyDown(Keys.S))
            translation += Vector3.Backward;

        if (keyboard.IsKeyDown(Keys.W))
            translation += Vector3.Forward;

        if (keyboard.IsKeyDown(Keys.Space))
            translation += Vector3.Up;

        if (keyboard.IsKeyDown(Keys.LeftShift))
            translation += Vector3.Down;

        return (keyboard.IsKeyDown(Keys.LeftControl) ? 8 : 1, translation);
    }

    private static (float, Vector3) GetGamepadTranslation(GamepadInputManager gamepad)
    {
        var translation = Vector3.Zero;
        var sticks = gamepad.State.ThumbSticks;
        var accelerate = 0.0f;

        if (gamepad.State.IsConnected)
        {
            var vertical = gamepad.State.Triggers.Right - gamepad.State.Triggers.Left;

            if (Math.Abs(vertical) > _controllerThreshold)
            {
                translation += new Vector3(sticks.Left.X, vertical, -sticks.Left.Y);
                accelerate = Math.Abs(vertical);
            }

            var horizontal = Math.Clamp(Math.Abs(sticks.Left.X) + Math.Abs(sticks.Left.Y), 0, 1);

            if (Math.Abs(horizontal) > _controllerThreshold)
            {
                if (!(Math.Abs(vertical) > _controllerThreshold))
                {
                    translation += new Vector3(sticks.Left.X, vertical, -sticks.Left.Y);
                }

                accelerate = Math.Clamp(Math.Abs(sticks.Left.X) + Math.Abs(sticks.Left.Y), 0, 1);
            }

            if (gamepad.IsButtonDown(Buttons.RightShoulder))
            {
                accelerate *= 8;
            }

            if (gamepad.IsButtonDown(Buttons.LeftShoulder))
            {
                accelerate /= 2;
            }
        }

        return (accelerate, translation);
    }

    private static void MoveCamera(Camera camera, Vector3 translation, float accelerate, TimeSpan elapsed)
    {
        var moveSpeed = _moveSpeed * (float)elapsed.TotalSeconds * 60;

        if (translation != Vector3.Zero)
        {
            var position = Vector3.Normalize(translation) * moveSpeed;

            if (Math.Abs(accelerate) < _moveThreshold)
            {
                camera.MoveRelativelyBy(position);
            }
            else
            {
                camera.MoveRelativelyBy(position * accelerate);
            }
        }
    }
}
