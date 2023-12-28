using CubeCity.EscSystems;
using CubeCity.Input;
using CubeCity.Managers;
using CubeCity.Models;
using CubeCity.Services;
using CubeCity.Systems;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CubeCity;

public class MainGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly TimeService _timeService;

    private Texture2D _blocksTexture = null!;

    private SpriteBatch _spriteBatch = null!;
    private SpriteFont _spriteFont = null!;

    private readonly EcsWorld _world;

    private IEcsSystems _update = null!;
    private IEcsSystems _draw = null!;

    public MainGame()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferMultiSampling = true,
            GraphicsProfile = GraphicsProfile.HiDef,
            SynchronizeWithVerticalRetrace = false
        };

        _timeService = new TimeService();

        //_gameServices = new GameServices(Window);
        //_gameSystems = new GameSystemManager();

        _world = new EcsWorld();

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

        //_gameServices.Initialize(GraphicsDevice);

        //_gameServices.BlocksController.ForceChunkGenerate(_gameServices.Camera.Position);

        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;

        _graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _blocksTexture = Content.Load<Texture2D>("Textures/Blocks");
        _spriteFont = Content.Load<SpriteFont>("GameFont");

        //_gameSystems.Add(new InputSystem(_gameServices));
        //_gameSystems.Add(new MoveSystem(_gameServices, Window, Exit, v => IsMouseVisible = v));
        //_gameSystems.Add(new ChunkSystem(_gameServices, _blocksTexture, GraphicsDevice));
        //_gameSystems.Add(new DisplayInfoSystem(_spriteBatch, _spriteFont, _gameServices));

        GraphicsDevice.Textures[0] = _blocksTexture;
        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

        GetUpdate();
    }

    protected override void Update(GameTime gameTime)
    {
        _timeService.AddTime(gameTime.ElapsedGameTime);
        //_gameSystems.Update(gameTime.ElapsedGameTime);

        _update.Run();

        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        //_gameSystems.Draw(gameTime.ElapsedGameTime);

        _draw.Run();

        base.Draw(gameTime);
    }

    private bool _disposed;

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
            _world.Destroy();
        }

        base.Dispose(disposing);
    }

    private void GetUpdate()
    {
        var settings = GameSettingsProvider.Settings;

        var gamepadManager = new GamepadInputManager();
        var keyboardManager = new KeyboardInputManager();
        var mouseManager = new MouseManager(Window)
        { 
            GamepadSensitivity = settings.GamepadSensitivity,
            MouseSensitivity = settings.MouseSensitivity
        };

        var camera = new GameObjects.Camera();

        var cameraSystem = new EcsCameraSystem(gamepadManager, keyboardManager, 
            mouseManager, camera, Window, _timeService);

        var chunkGenerator = new EcsChunkGeneratorSystem(camera, settings.ChunksViewDistance,
            new Generators.Pipelines.ChunkGenerator(new Tools.PerlinNoise2D(), 
                settings.Blocks, settings.GeneratingChunkThreads, GraphicsDevice));

        var display = new EcsDisplayInfoSystem(gamepadManager, keyboardManager, _spriteBatch, _spriteFont, camera);

        var rasterizer = new RasterizerState { CullMode = CullMode.CullClockwiseFace, MultiSampleAntiAlias = true };

        var effect = new BasicEffect(GraphicsDevice)
        {
            TextureEnabled = true,
            PreferPerPixelLighting = true
        };

        var render = new EcsRenderSystem(GraphicsDevice, rasterizer, camera, effect, _blocksTexture);

        _update = new EcsSystems(_world);

        _update
            .Add(cameraSystem)
            .Add(chunkGenerator)
            .Add(display)
            .Init();

        _draw = new EcsSystems(_world);

        _draw
            .Add(render)
            .Init();
    }
}
