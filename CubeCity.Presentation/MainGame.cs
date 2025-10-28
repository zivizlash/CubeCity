using CubeCity.Builders;
using CubeCity.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CubeCity;

public class MainGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly TimeService _timeService;
    private readonly ILoggerFactory _loggerFactory;

    private Texture2D _blocksTexture = null!;
    private SpriteBatch _spriteBatch = null!;
    private SpriteFont _spriteFont = null!;

    private GameSystemsContainer _gameSystems = null!;

    private bool _disposed;

    public MainGame(ILoggerFactory loggerFactory)
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferMultiSampling = true,
            GraphicsProfile = GraphicsProfile.HiDef,
            SynchronizeWithVerticalRetrace = false
        };

        _timeService = new TimeService();

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1) / 170;
        IsFixedTimeStep = true;
        Window.AllowUserResizing = true;
        _loggerFactory = loggerFactory;
    }

    protected override void Initialize()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;

        _graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _blocksTexture = Content.Load<Texture2D>("Textures/Blocks");
        _spriteFont = Content.Load<SpriteFont>("GameFont");

        GraphicsDevice.Textures[0] = _blocksTexture;
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

        _gameSystems = BuildSystems();
    }

    protected override void Update(GameTime gameTime)
    {
        _timeService.AddTime(gameTime.ElapsedGameTime);
        _gameSystems.UpdateSystems.Run();
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _gameSystems.DrawSystems.Run();
        base.Draw(gameTime);
    }

    private GameSystemsContainer BuildSystems()
    {
        var builder = new GameSystemsBuilder();

        var args = new GameData(_blocksTexture, _spriteBatch, _spriteFont, _graphics, _timeService, 
            GameSettingsProvider.Settings, Window, GraphicsDevice, Exit, _loggerFactory);

        return builder.Build(args);
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
            _gameSystems.World.Destroy();
        }

        base.Dispose(disposing);
    }
}
