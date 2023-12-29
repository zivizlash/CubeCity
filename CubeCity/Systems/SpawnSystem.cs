using CubeCity.Components;
using CubeCity.GameObjects;
using CubeCity.Input;
using CubeCity.Models;
using Leopotam.EcsLite;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeCity.Systems;

public class SpawnSystem : IEcsInitSystem, IEcsRunSystem
{
    private readonly Camera _camera;
    private readonly KeyboardInputManager _keyboardManager;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly BlockType[] _blockTypes;
    private readonly ILogger<SpawnSystem> _logger;

    private EcsPool<PhysicsComponent> _physicsPool = null!;
    private EcsPool<PositionComponent> _positionPool = null!;
    private EcsPool<RenderComponent> _renderPool = null!;

    public SpawnSystem(Camera camera, KeyboardInputManager keyboardManager, 
        GraphicsDevice graphicsDevice, BlockType[] blockTypes, ILogger<SpawnSystem> logger)
    {
        _camera = camera;
        _keyboardManager = keyboardManager;
        _graphicsDevice = graphicsDevice;
        _blockTypes = blockTypes;
        _logger = logger;
    }

    public void Init(IEcsSystems systems)
    {
        var world = systems.GetWorld();

        _physicsPool = world.GetPool<PhysicsComponent>();
        _positionPool = world.GetPool<PositionComponent>();
        _renderPool = world.GetPool<RenderComponent>();
    }

    public void Run(IEcsSystems systems)
    {
        if (_keyboardManager.IsKeyPressed(Keys.L))
        {
            var world = systems.GetWorld();
            var entity = world.NewEntity();

            var (vertexBuffer, indexBuffer) = GenerateCube();

            ref var physics = ref _physicsPool.Add(entity);
            physics.Velocity = Vector3.Up;

            ref var position = ref _positionPool.Add(entity);
            position.Position = _camera.Position + _camera.Forward * 2 + Vector3.Up * 2 ;

            ref var render = ref _renderPool.Add(entity);
            render.VertexBuffer = vertexBuffer;
            render.IndexBuffer = indexBuffer;

            _logger.LogInformation("Created entity at {pos}", position.Position);
        }
    }

    private (VertexBuffer, IndexBuffer) GenerateCube()
    {
        var cube = new CubeGenerator(_blockTypes[0]);
        var vertices = new VertexPositionTexture[cube.Vertices.Length];

        for (int vertexIndex = 0; vertexIndex < cube.Vertices.Length; vertexIndex++)
        {
            vertices[vertexIndex] = new VertexPositionTexture(
                cube.Vertices[vertexIndex], cube.Uvs[vertexIndex]);
        }

        var indexBuffer = new IndexBuffer(_graphicsDevice, 
            IndexElementSize.ThirtyTwoBits, cube.Triangles.Length, BufferUsage.None);

        var vertexBuffer = new VertexBuffer(_graphicsDevice,
            typeof(VertexPositionTexture), cube.Uvs.Length, BufferUsage.None);

        indexBuffer.SetData(cube.Triangles);
        vertexBuffer.SetData(vertices);

        return (vertexBuffer, indexBuffer);
    }
}
