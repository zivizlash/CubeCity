using CubeCity.GameObjects;
using CubeCity.Generators.Algs;
using CubeCity.Generators.Chunks;
using CubeCity.Generators.Pipelines;
using CubeCity.Input;
using CubeCity.Services;
using CubeCity.Systems.Chunks;
using CubeCity.Systems.Input;
using CubeCity.Systems.Physics;
using CubeCity.Systems.Render;
using CubeCity.Threading;
using CubeCity.Tools;
using Leopotam.EcsLite;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;

namespace CubeCity.Builders;

public class GameSystemsBuilder
{
    public GameSystemsContainer Build(GameData gameData)
    {
        var world = new EcsWorld();
        var settings = GameSettingsProvider.Settings;
        var camera = new Camera();

        var gamepadManager = new GamepadInputManager();
        var keyboardManager = new KeyboardInputManager();
        var mouseManager = new MouseService(gameData.Window)
        {
            GamepadSensitivity = settings.GamepadSensitivity,
            MouseSensitivity = settings.MouseSensitivity
        };

        var backgroundManager = new BackgroundManager();

        var inputSystem = CreateInputSystem(gamepadManager, keyboardManager, mouseManager);
        var cameraSystem = CreateCameraSystem(gameData, camera, gamepadManager, keyboardManager, mouseManager);
        var (playerLoaderSystem, chunkSystem) = CreateChunkSystem(gameData, world, settings, camera, backgroundManager, gameData.LoggerFactory);
        var displaySystem = CreateDisplayInfoSystem(gameData, camera, gamepadManager, keyboardManager);
        var spawnSystem = CreateSpawnSystem(world, gameData, camera, keyboardManager);
        var physicsSystem = CreatePhysicsSystem(world, gameData);
        var renderSystem = CreateRenderSystem(world, gameData, camera);

        var updateSystems = new EcsSystems(world)
            .Add(inputSystem)
            .Add(cameraSystem)
            .Add(spawnSystem)
            .Add(playerLoaderSystem)
            .Add(chunkSystem)
            .Add(physicsSystem)
            .InitChain();

        var drawSystems = new EcsSystems(world)
            .Add(renderSystem)
            .Add(displaySystem)
            .InitChain();

        return new GameSystemsContainer(updateSystems, drawSystems, world);
    }

    private static InputSystem CreateInputSystem(GamepadInputManager gamepadManager, KeyboardInputManager keyboardManager, MouseService mouseManager)
    {
        return new InputSystem(gamepadManager, keyboardManager, mouseManager);
    }

    private static CameraSystem CreateCameraSystem(GameData gameData, Camera camera, GamepadInputManager gamepadManager, KeyboardInputManager keyboardManager, MouseService mouseManager)
    {
        return new CameraSystem(gamepadManager, keyboardManager,
            mouseManager, camera, gameData.Window, gameData.Time);
    }

    private static DisplayInfoSystem CreateDisplayInfoSystem(GameData gameData, Camera camera, GamepadInputManager gamepadManager, KeyboardInputManager keyboardManager)
    {
        return new DisplayInfoSystem(gamepadManager, keyboardManager,
            gameData.SpriteBatch, gameData.SpriteFont, camera, gameData.Exit);
    }

    private static SpawnSystem CreateSpawnSystem(EcsWorld world, GameData gameData, Camera camera, KeyboardInputManager keyboardManager)
    {
        return new SpawnSystem(world, camera, keyboardManager,
            gameData.GraphicsDevice, gameData.Settings.Blocks,
            gameData.LoggerFactory.CreateLogger<SpawnSystem>());
    }

    private static PhysicsSystem CreatePhysicsSystem(EcsWorld world, GameData gameData)
    {
        return new PhysicsSystem(world, gameData.Time);
    }

    private static RenderSystem CreateRenderSystem(EcsWorld world, GameData gameData, Camera camera)
    {
        var rasterizer = new RasterizerState
        {
            CullMode = CullMode.CullClockwiseFace,
            MultiSampleAntiAlias = true
        };

        var effect = new BasicEffect(gameData.GraphicsDevice)
        {
            TextureEnabled = true,
            PreferPerPixelLighting = true
        };

        var renderSystem = new RenderSystem(world, gameData.GraphicsDevice, rasterizer,
            camera, effect, gameData.BlocksTexture);

        return renderSystem;
    }

    private static (ChunkPlayerAroundLoaderSystem, ChunkGeneratorSystem) CreateChunkSystem(
        GameData gameData, EcsWorld world, GameSettings settings, Camera camera,
        BackgroundManager backgroundManager, ILoggerFactory loggerFactory)
    {
        var chunkIsRequiredChecker = new ChunkIsRequiredChecker(
            settings.ChunksUnloadDistance, settings.ChunksViewDistance);

        var chunkBlockGenerator = new CompositeChunkBlocksGenerator([
            new PerlinChunkBlocksGenerator(new PerlinNoise2D()),
            new DiamondSquareChunkBlocksGenerator()]);

        var chunkGenerator = new ChunkGenerator(settings.Blocks, gameData.GraphicsDevice, chunkBlockGenerator);

        var chunkLoaderLogger = loggerFactory.CreateLogger<ChunkGeneratorSystem>();

        var playerLoader = new ChunkPlayerAroundLoaderSystem(world, camera, chunkIsRequiredChecker);
        var chunkLoader = new ChunkGeneratorSystem(world, chunkGenerator, backgroundManager, chunkLoaderLogger);

        return (playerLoader, chunkLoader);
    }
}
