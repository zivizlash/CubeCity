using CubeCity.GameObjects;
using CubeCity.Generators.Algs;
using CubeCity.Generators.Pipelines;
using CubeCity.Input;
using CubeCity.Services;
using CubeCity.Systems;
using CubeCity.Tools;
using Leopotam.EcsLite;
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

        var cameraSystem = new CameraSystem(gamepadManager, keyboardManager,
            mouseManager, camera, gameData.Window, gameData.Time);

        var chunkGenerator = new ChunkGeneratorSystem(camera, settings.ChunksViewDistance,
            new ChunkBlockGenerator(new PerlinNoise2D(),
                settings.Blocks, settings.GeneratingChunkThreads, gameData.GraphicsDevice));

        var display = new DisplayInfoSystem(gamepadManager, keyboardManager,
            gameData.SpriteBatch, gameData.SpriteFont, camera, gameData.Exit);

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

        var render = new RenderSystem(gameData.GraphicsDevice, rasterizer,
            camera, effect, gameData.BlocksTexture);

        var updateSystems = new EcsSystems(world)
            .Add(cameraSystem)
            .Add(chunkGenerator)
            .Add(display)
            .InitChain();

        var drawSystems = new EcsSystems(world)
            .Add(render)
            .InitChain();

        return new GameSystemsContainer(updateSystems, drawSystems, world);
    }
}
