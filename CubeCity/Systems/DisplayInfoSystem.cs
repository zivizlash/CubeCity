﻿using CubeCity.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime;

namespace CubeCity.Systems
{
    public class DisplayInfoSystem : IGameUpdateSystem, IGameDrawSystem
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly SpriteFont _spriteFont;
        private readonly GameServices _gameServices;

        private bool _displayInformation = true;

        public DisplayInfoSystem(SpriteBatch spriteBatch, SpriteFont spriteFont, GameServices gameServices)
        {
            _spriteBatch = spriteBatch;
            _spriteFont = spriteFont;
            _gameServices = gameServices;
        }

        public void Update(TimeSpan elapsed)
        {
            var gamepad = _gameServices.GamepadManager;
            var keyboard = _gameServices.KeyboardManager;

            if (gamepad.IsButtonPressed(Buttons.Y) || keyboard.IsKeyPressed(Keys.Y))
                _displayInformation = !_displayInformation;

            if (keyboard.IsKeyPressed(Keys.J))
                GC.Collect(2, GCCollectionMode.Forced, false, true);
        }
         
        public void Draw(TimeSpan elapsed)
        {
            if (_displayInformation)
            {
                _spriteBatch.Begin();

                _spriteBatch.DrawString(_spriteFont,
                    $"GC Latency Mode: {GCSettings.LatencyMode}; " + 
                    $"Gravity: {_gameServices.UseCameraGravity}; " +
                    $"Chunks Pool: {ChunkBlocksPool.CreatedArrays}; " + 
                    $"Chunks: {_gameServices.ChunkManager.ChunksCount}; " +
                    $"FPS: {(int)Math.Round(0.0)}; " +
                    $"Heap: {GC.GetGCMemoryInfo().HeapSizeBytes / 1024}k " +
                    $"Position: {_gameServices.Camera.Position}; " +
                    $"Look At: {_gameServices.Camera.Forward}; ",
                    Vector2.One, Color.Black);

                _spriteBatch.End();
            }
        }
    }
}