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

        var commonServices = new CommonServices
        {
            World = world,
            Settings = settings,
            BackgroundManager = backgroundManager,
            GamepadManager = gamepadManager,
            KeyboardManager = keyboardManager,
            MouseService = mouseManager,
            GameData = gameData
        };

        var inputSystem = CreateInputSystem(commonServices);
        var cameraSystem = CreateCameraSystem(commonServices, camera);
        var chunkSystems =  CreateChunkSystem(commonServices, camera);
        var displaySystem = CreateDisplayInfoSystem(commonServices, camera);
        var spawnSystem = CreateSpawnSystem(commonServices, camera);
        var physicsSystem = CreatePhysicsSystem(commonServices);
        var renderSystem = CreateRenderSystem(commonServices, camera);

        var updateSystems = new EcsSystems(world)
            .Add(inputSystem)
            .Add(cameraSystem)
            .Add(spawnSystem)
            .Add(chunkSystems.ChunkPlayerLoader)
            .Add(chunkSystems.ChunkGeneratorSystem)
            .Add(chunkSystems.ChunkUpdatingSystem)
            .Add(chunkSystems.ChunkMeshSystem)
            .Add(physicsSystem)
            .InitChain();

        var drawSystems = new EcsSystems(world)
            .Add(renderSystem)
            .Add(displaySystem)
            .InitChain();

        return new GameSystemsContainer(updateSystems, drawSystems, world);
    }

    private static InputSystem CreateInputSystem(CommonServices services)
    {
        return new InputSystem(services.GamepadManager, services.KeyboardManager, services.MouseService);
    }

    private static CameraSystem CreateCameraSystem(CommonServices services, Camera camera)
    {
        return new CameraSystem(services.GamepadManager, services.KeyboardManager,
            services.MouseService, camera, services.GameData.Window, services.GameData.Time);
    }

    private static DisplayInfoSystem CreateDisplayInfoSystem(CommonServices services, Camera camera)
    {
        return new DisplayInfoSystem(services.GamepadManager, services.KeyboardManager,
            services.GameData.SpriteBatch, services.GameData.SpriteFont, camera, services.GameData.Exit);
    }

    private static SpawnSystem CreateSpawnSystem(CommonServices services, Camera camera)
    {
        return new SpawnSystem(services.World, camera, services.KeyboardManager,
            services.GameData.GraphicsDevice, services.GameData.Settings.Blocks,
            services.GameData.LoggerFactory.CreateLogger<SpawnSystem>());
    }

    private static PhysicsSystem CreatePhysicsSystem(CommonServices services)
    {
        return new PhysicsSystem(services.World, services.GameData.Time);
    }

    private static RenderSystem CreateRenderSystem(CommonServices services, Camera camera)
    {
        var rasterizer = new RasterizerState
        {
            CullMode = CullMode.CullClockwiseFace,
            MultiSampleAntiAlias = true
        };

        var effect = new BasicEffect(services.GameData.GraphicsDevice)
        {
            TextureEnabled = true,
            PreferPerPixelLighting = true
        };

        var renderSystem = new RenderSystem(services.World, services.GameData.GraphicsDevice, rasterizer,
            camera, effect, services.GameData.BlocksTexture);

        return renderSystem;
    }

    private static ChunkSystemsData CreateChunkSystem(CommonServices services, Camera camera)
    {
        var chunkIsRequiredChecker = new ChunkIsRequiredChecker(
            services.Settings.ChunksUnloadDistance, services.Settings.ChunksViewDistance);

        var chunkBlockGenerator = new CompositeChunkBlocksGenerator([
            new PerlinChunkBlocksGenerator(new PerlinNoise2D()),
            new DiamondSquareChunkBlocksGenerator()]);

        var chunkGenerator = new ChunkGenerator(services.Settings.Blocks, services.GameData.GraphicsDevice, chunkBlockGenerator);
        var playerLoader = new ChunkPlayerLoaderSystem(services.World, camera, chunkIsRequiredChecker);
        var chunkGeneratorSystem = new ChunkGeneratorSystem(services.World, chunkBlockGenerator, services.BackgroundManager);
        var chunkUpdatingSystem = new ChunkUpdatingSystem(services.World);
        var chunkMeshSystem = new ChunkMeshSystem(services.World, services.BackgroundManager,
            services.GameData.GraphicsDevice, services.Settings.Blocks, 
            services.GameData.LoggerFactory.CreateLogger<ChunkMeshSystem>());

        return new ChunkSystemsData
        {
            ChunkPlayerLoader = playerLoader,
            ChunkGeneratorSystem = chunkGeneratorSystem,
            ChunkUpdatingSystem = chunkUpdatingSystem,
            ChunkMeshSystem = chunkMeshSystem
        };
    }

    internal struct ChunkSystemsData
    {
        public required ChunkPlayerLoaderSystem ChunkPlayerLoader;
        public required ChunkGeneratorSystem ChunkGeneratorSystem;
        public required ChunkUpdatingSystem ChunkUpdatingSystem;
        public required ChunkMeshSystem ChunkMeshSystem;
    }

    internal struct CommonServices
    {
        public required EcsWorld World;
        public required GameData GameData;
        public required GamepadInputManager GamepadManager;
        public required KeyboardInputManager KeyboardManager;
        public required MouseService MouseService;
        public required GameSettings Settings;
        public required BackgroundManager BackgroundManager;
    }
}
