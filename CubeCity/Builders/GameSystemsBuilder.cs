using CubeCity.GameObjects;
using CubeCity.Generators.Algs;
using CubeCity.Generators.Chunks;
using CubeCity.Generators.Pipelines;
using CubeCity.Input;
using CubeCity.Services;
using CubeCity.Systems;
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

        var gamepadManager = new GamepadInputManager();
        var keyboardManager = new KeyboardInputManager();
        var mouseManager = new MouseService(gameData.Window)
        {
            GamepadSensitivity = settings.GamepadSensitivity,
            MouseSensitivity = settings.MouseSensitivity
        };

        var camera = new Camera();

        var inputSystem = new InputSystem(gamepadManager, keyboardManager, mouseManager);

        var cameraSystem = new CameraSystem(gamepadManager, keyboardManager,
            mouseManager, camera, gameData.Window, gameData.Time);

        var chunkIsRequiredChecker = new ChunkIsRequiredChecker(settings.ChunksViewDistance + 2);

        var chunkGenerator = new CompositeChunkBlocksGenerator([
            new PerlinChunkBlocksGenerator(new PerlinNoise2D()),
            new DiamondSquareChunkBlocksGenerator()]);

        var chunkSystem = new ChunkGeneratorSystem(
            camera, settings.ChunksViewDistance, chunkIsRequiredChecker,
            new ChunkBlockGenerator(settings.Blocks, settings.GeneratingChunkThreads, 
                gameData.GraphicsDevice, chunkGenerator, chunkIsRequiredChecker));

        var displaySystem = new DisplayInfoSystem(gamepadManager, keyboardManager,
            gameData.SpriteBatch, gameData.SpriteFont, camera, gameData.Exit);

        var spawnSystem = new SpawnSystem(camera, keyboardManager, 
            gameData.GraphicsDevice, gameData.Settings.Blocks, 
            gameData.LoggerFactory.CreateLogger<SpawnSystem>());

        var physicsSystem = new PhysicsSystem(gameData.Time);

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

        var renderSystem = new RenderSystem(gameData.GraphicsDevice, rasterizer,
            camera, effect, gameData.BlocksTexture);

        var updateSystems = new EcsSystems(world)
            .Add(inputSystem)
            .Add(cameraSystem)
            .Add(spawnSystem)
            .Add(chunkSystem)
            .Add(physicsSystem)
            .InitChain();

        var drawSystems = new EcsSystems(world)
            .Add(renderSystem)
            .Add(displaySystem)
            .InitChain();

        return new GameSystemsContainer(updateSystems, drawSystems, world);
    }
}
