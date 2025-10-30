using CubeCity.GameObjects;
using CubeCity.Input;
using CubeCity.Models;
using CubeCity.Tools;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime;

namespace CubeCity.Systems.Render;

public class DisplayInfoSystem(GamepadInputManager gamepadManager, KeyboardInputManager keyboardManager,
    SpriteBatch spriteBatch, SpriteFont spriteFont, Camera camera, Action exit) : IEcsRunSystem
{
    private bool _displayInformation = true;

    public void Run(IEcsSystems systems)
    {
        if (gamepadManager.IsButtonPressed(Buttons.B) || keyboardManager.IsKeyDown(Keys.Escape))
        {
            exit.Invoke();
        }

        if (gamepadManager.IsButtonPressed(Buttons.Y) || keyboardManager.IsKeyPressed(Keys.Y))
        {
            _displayInformation = !_displayInformation;
        }

        if (keyboardManager.IsKeyPressed(Keys.J))
        {
            GC.Collect(2, GCCollectionMode.Forced, false, true);
        }

        if (_displayInformation)
        {
            spriteBatch.Begin();

            var pos = camera.Position;
            var chunkPos = BlocksTools.GetChunkPosByWorld(pos);
            var blockPos = new Vector3Int((int)pos.X % 16, (int)pos.Y % 16, (int)pos.Z % 16);

            spriteBatch.DrawString(spriteFont,
                $"GC Latency Mode: {GCSettings.LatencyMode}; " +
                $"Chunks Pool: {ChunkBlocksPool.CreatedArrays}; " +
                $"Heap: {GC.GetGCMemoryInfo().HeapSizeBytes / 1024}k " +
                $"Position: {camera.Position}; " +
                $"Look At: {camera.Forward}; " +
                $"Chunk Pos: {chunkPos}; " +
                $"Block Pos: {blockPos}; ",
                Vector2.One, Color.Black);

            spriteBatch.End();
        }
    }
}
