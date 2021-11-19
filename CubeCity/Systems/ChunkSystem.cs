using System;
using CubeCity.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeCity.Systems
{
    public class ChunkSystem : IGameUpdateSystem, IGameDrawSystem
    {
        private readonly GameServices _gameServices;
        private readonly Texture2D _texture;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly BasicEffect _effect;

        public ChunkSystem(GameServices gameServices, Texture2D texture, GraphicsDevice graphicsDevice)
        {
            _gameServices = gameServices;
            _texture = texture;
            _graphicsDevice = graphicsDevice;
            _effect = new BasicEffect(graphicsDevice)
            {
                TextureEnabled = true,
                PreferPerPixelLighting = true
            };
        }

        public void Update(TimeSpan elapsed)
        {
            _gameServices.BlocksController.UpdatePlayerPosition(_gameServices.Camera.Position);
            _gameServices.ChunkManager.Update(elapsed);
        }

        public void Draw(TimeSpan elapsed)
        {
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = _gameServices.RasterizerState;

            var world = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);

            _effect.View = _gameServices.Camera.ViewMatrix;
            _effect.Projection = _gameServices.Camera.ProjectionMatrix;
            _effect.Texture = _texture;

            _gameServices.ChunkManager.Draw(world, _effect);
        }
    }
}
