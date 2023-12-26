using CubeCity.GameObjects;
using CubeCity.Input;
using CubeCity.Managers;
using CubeCity.Models;
using CubeCity.Tools;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime;

namespace CubeCity.EscSystems;

public class EcsDisplayInfoSystem : IEcsRunSystem
{
    private readonly GamepadInputManager _gamepadManager;
    private readonly KeyboardInputManager _keyboardManager;
    private readonly SpriteBatch _spriteBatch;
    private readonly SpriteFont _spriteFont;
    private readonly Camera _camera;

    private bool _displayInformation;

    public EcsDisplayInfoSystem(GamepadInputManager gamepadManager, KeyboardInputManager keyboardManager,
        SpriteBatch spriteBatch, SpriteFont spriteFont, Camera camera)
    {
        _gamepadManager = gamepadManager;
        _keyboardManager = keyboardManager;
        _spriteBatch = spriteBatch;
        _spriteFont = spriteFont;
        _camera = camera;
    }

    public void Run(IEcsSystems systems)
    {
        // Вообще вроде как для обновления управление используется не Draw, а Update, но

        if (_gamepadManager.IsButtonPressed(Buttons.Y) || _keyboardManager.IsKeyPressed(Keys.Y))
            _displayInformation = !_displayInformation;

        //if (keyboard.IsKeyPressed(Keys.R))
        //    _gameServices.BlocksController.RegenerateCurrentChunk(_gameServices.Camera.Position);

        if (_keyboardManager.IsKeyPressed(Keys.J))
            GC.Collect(2, GCCollectionMode.Forced, false, true);

        if (_displayInformation)
        {
            _spriteBatch.Begin();

            var pos = _camera.Position;
            var chunkPos = BlocksEnvironmentController.GetChunkPosByPlayerPos(pos);
            var blockPos = new Vector3Int((int)pos.X % 16, (int)pos.Y % 16, (int)pos.Z % 16);

            _spriteBatch.DrawString(_spriteFont,
                $"GC Latency Mode: {GCSettings.LatencyMode}; " +
                $"Chunks Pool: {ChunkBlocksPool.CreatedArrays}; " +
                $"Heap: {GC.GetGCMemoryInfo().HeapSizeBytes / 1024}k " +
                $"Position: {_camera.Position}; " +
                $"Look At: {_camera.Forward}; " +
                $"Chunk Pos: {chunkPos}; " +
                $"Block Pos: {blockPos}; ",
                Vector2.One, Color.Black);

            _spriteBatch.End();
        }
    }
}
