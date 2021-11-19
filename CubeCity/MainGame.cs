using CubeCity.Models;
using CubeCity.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CubeCity
{
    public class MainGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private Texture2D _blocksTexture;

        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;

        private readonly GameServices _gameServices;
        private readonly GameSystemManager _gameSystems;

        public MainGame()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferMultiSampling = true,
                GraphicsProfile = GraphicsProfile.HiDef,
                SynchronizeWithVerticalRetrace = false
            };

            _gameServices = new GameServices(Window);
            _gameSystems = new GameSystemManager();

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            TargetElapsedTime = TimeSpan.FromSeconds(1) / 170;
            IsFixedTimeStep = true;
            Window.AllowUserResizing = true;
        }

        protected override void Initialize()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            _gameServices.Initialize(GraphicsDevice);

            _gameServices.BlocksController.ForceChunkGenerate(
                _gameServices.Camera.Position);

            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;

            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _blocksTexture = Content.Load<Texture2D>("Textures/Blocks");
            _spriteFont = Content.Load<SpriteFont>("GameFont");

            _gameSystems.Add(new InputSystem(_gameServices));
            _gameSystems.Add(new MoveSystem(_gameServices, Window, Exit, v => IsMouseVisible = v));
            _gameSystems.Add(new ChunkSystem(_gameServices, _blocksTexture, GraphicsDevice));
            _gameSystems.Add(new DisplayInfoSystem(_spriteBatch, _spriteFont, _gameServices));

            GraphicsDevice.Textures[0] = _blocksTexture;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        }

        protected override void Update(GameTime gameTime)
        {
            _gameSystems.Update(gameTime.ElapsedGameTime);
            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _gameSystems.Draw(gameTime.ElapsedGameTime);
            base.Draw(gameTime);
        }
    }
}
