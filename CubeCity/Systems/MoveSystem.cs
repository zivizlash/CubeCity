using CubeCity.GameObjects;
using CubeCity.Input;
using CubeCity.Tools;
using CubeCity.Managers;
using CubeCity.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace CubeCity.Systems
{
    public class MoveSystem : IGameUpdateSystem
    {
        private readonly GameServices _gameServices;
        private readonly GameWindow _gameWindow;

        private readonly Action _exit;
        private readonly Action<bool> _isMouseVisible;

        private float _verticalSpeed;

        public MoveSystem(GameServices gameServices, GameWindow gameWindow,
            Action exit, Action<bool> isMouseVisible)
        {
            _gameServices = gameServices;
            _gameWindow = gameWindow;
            _exit = exit;
            _isMouseVisible = isMouseVisible;
        }

        public ushort GetBlockByPos(Vector3 pos)
        {
            var chunkPos = BlocksEnvironmentController.GetChunkPosByPlayerPos(pos);
            var chunk = _gameServices.ChunkManager.GenerateChunk(chunkPos);

            if (chunk.IsInWorld)
            {
                var blocks = chunk.Blocks!;

                var posInChunk = new Vector3Int(
                    ((int)pos.X + 0x10000000) % 16,
                    (int)pos.Y % 128,
                    ((int)pos.Z + 0x10000000) % 16);
                
                return pos.Y is >= 0 and <= 128
                    ? blocks[posInChunk.X, posInChunk.Y, posInChunk.Z]
                    : (ushort)0;
            }

            return 0;
        }

        public void Update(TimeSpan elapsed)
        {
            var gamepad = _gameServices.GamepadManager;
            var keyboard = _gameServices.KeyboardManager;
            var camera = _gameServices.Camera;
            var sticks = gamepad.State.ThumbSticks;

            if (keyboard.IsKeyPressed(Keys.F))
                _gameServices.UseCameraGravity = !_gameServices.UseCameraGravity;

            UpdateCamera(camera, elapsed, sticks, Mouse.GetState(),
                _gameServices.MouseManager, _gameWindow, _isMouseVisible);

            UpdateWorld(keyboard, gamepad);

            var (gamepadAccelerate, gamepadTranslation) = GetGamepadTranslation(gamepad, sticks);
            var (keyboardAccelerate, keyboardTranslation) = GetKeyboardTranslation(keyboard);

            var translation = gamepadTranslation + keyboardTranslation;
            var accelerate = gamepadAccelerate + keyboardAccelerate;

            if (_gameServices.UseCameraGravity)
            {
                var footPos = camera.Position;

                footPos.Y -= 5;
                var below = GetBlockByPos(footPos) != 0;
                footPos.Y += 1;
                var above = GetBlockByPos(footPos) != 0;

                if (above)
                {
                    _verticalSpeed = Math.Clamp(_verticalSpeed, 0, int.MaxValue);
                    _verticalSpeed += 1.2f * (float)elapsed.TotalSeconds;
                }

                if (below == false)
                    _verticalSpeed -= 0.2f * (float)elapsed.TotalSeconds;
                
                translation.Y = 0;
                camera.MoveRelativelyBy(new Vector3(0, _verticalSpeed, 0));
            }

            MoveCamera(camera, translation, accelerate, elapsed);
        }

        private void UpdateWorld(KeyboardInputManager keyboard, GamepadInputManager gamepad)
        {
            var chunkGenerator = _gameServices.ChunkGenerator;

            if (gamepad.IsButtonPressed(Buttons.X) || keyboard.IsKeyPressed(Keys.U))
                chunkGenerator.UsePerlinNoise = !chunkGenerator.UsePerlinNoise;

            if (gamepad.IsButtonPressed(Buttons.B) || keyboard.IsKeyPressed(Keys.Escape))
            {
                PerformanceCounter.SaveReport();
                _exit.Invoke();
            }
        }

        private static void UpdateCamera(Camera camera, TimeSpan elapsed,
            GamePadThumbSticks sticks, MouseState mouse, MouseManager mouseManager,
            GameWindow gameWindow, Action<bool> mouseVisible)
        {
            mouseManager.IsCaptured = mouse.MiddleButton == ButtonState.Pressed;
            mouseVisible.Invoke(!mouseManager.IsCaptured);

            var rotation = mouseManager.GetRotation(
                mouse.Position, elapsed, sticks.Right.Y, sticks.Right.X);

            camera.SetClientBounds(gameWindow.ClientBounds);
            camera.SetCameraForward(rotation);
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

        private static (float, Vector3) GetGamepadTranslation(GamepadInputManager gamepad, GamePadThumbSticks sticks)
        {
            var translation = Vector3.Zero;
            var accelerate = 0.0f;

            if (gamepad.State.IsConnected)
            {
                var vertical = gamepad.State.Triggers.Right - gamepad.State.Triggers.Left;

                if (Math.Abs(vertical) > 0.001)
                {
                    translation += new Vector3(sticks.Left.X, vertical, -sticks.Left.Y);
                    accelerate = Math.Abs(vertical);
                }

                var horizontal = Math.Clamp(Math.Abs(sticks.Left.X) + Math.Abs(sticks.Left.Y), 0, 1);

                if (Math.Abs(horizontal) > 0.001)
                {
                    if (!(Math.Abs(vertical) > 0.001))
                        translation += new Vector3(sticks.Left.X, vertical, -sticks.Left.Y);

                    accelerate = Math.Clamp(
                        Math.Abs(sticks.Left.X) + Math.Abs(sticks.Left.Y), 0, 1);
                }

                if (gamepad.IsButtonDown(Buttons.RightShoulder))
                    accelerate *= 8;

                if (gamepad.IsButtonDown(Buttons.LeftShoulder))
                    accelerate /= 2;
            }

            return (accelerate, translation);
        }

        private static void MoveCamera(Camera camera, Vector3 translation, float accelerate, TimeSpan elapsed)
        {
            var moveSpeed = 0.5f * (float)elapsed.TotalSeconds * 60;

            if (translation != Vector3.Zero)
            {
                var position = Vector3.Normalize(translation) * moveSpeed;

                if (Math.Abs(accelerate) < 0.001)
                    camera.MoveRelativelyBy(position);
                else
                    camera.MoveRelativelyBy(position * accelerate);
            }
        }
    }
}
