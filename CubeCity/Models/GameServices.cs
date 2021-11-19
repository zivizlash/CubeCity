using CubeCity.GameObjects;
using CubeCity.Generators.Pipelines;
using CubeCity.Input;
using CubeCity.Managers;
using CubeCity.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Text.Json;
using CubeCity.Pools;
using Microsoft.Extensions.Logging;

namespace CubeCity.Models
{
    public class GameServices
    {
        public Camera Camera { get; }
        public MouseManager MouseManager { get; }
        public ChunkGenerator ChunkGenerator { get; private set; }
        public ChunkManager ChunkManager { get; private set; }
        public BlocksEnvironmentController BlocksController { get; private set; }
        public RasterizerState RasterizerState { get; }
        public KeyboardInputManager KeyboardManager { get; }
        public GamepadInputManager GamepadManager { get; }
        public GameSettings Settings { get; }
        public bool UseCameraGravity { get; set; }

        public ILoggerFactory GameLoggerFactory { get; }

        public GameServices(GameWindow window)
        {
            GameLoggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
            var logger = GameLoggerFactory.CreateLogger<GameServices>();
            
            Settings = JsonSerializer.Deserialize<GameSettings>(File.ReadAllText("appsettings.json"))
                ?? throw new InvalidOperationException();

            logger.LogInformation(
                "Loaded settings: ViewDistance {ViewDistance}; Generator Threads {GeneratorThreads};", 
                Settings.ChunksViewDistance, Settings.GeneratingChunkThreads);

            GraphicsGeneratorItemsPool.Instance.SetupLogger(GameLoggerFactory.CreateLogger<GraphicsGeneratorItemsPool>());

            Camera = new Camera();

            MouseManager = new MouseManager(window)
            {
                MouseSensitivity = Settings.MouseSensitivity,
                GamepadSensitivity = Settings.GamepadSensitivity
            };

            RasterizerState = new RasterizerState
            {
                CullMode = CullMode.CullClockwiseFace,
                MultiSampleAntiAlias = true
            };

            KeyboardManager = new KeyboardInputManager();
            GamepadManager = new GamepadInputManager();
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            ChunkGenerator = new ChunkGenerator(
                new PerlinNoise2D(), Settings.Blocks, Settings.GeneratingChunkThreads, graphicsDevice);

            ChunkManager = new ChunkManager(ChunkGenerator, graphicsDevice);
            BlocksController = new BlocksEnvironmentController(
                ChunkManager, new Vector3(0, 64, 0), Settings.ChunksViewDistance);
        }
    }
}
