using CubeCity.Components;
using CubeCity.GameObjects;
using CubeCity.Generators.Algs;
using CubeCity.Input;
using CubeCity.Models;
using Leopotam.EcsLite;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeCity.Systems;

public class SpawnSystem(EcsWorld world, Camera camera, KeyboardInputManager keyboardManager,
    GraphicsDevice graphicsDevice, BlockType[] blockTypes, ILogger<SpawnSystem> logger) 
    : IEcsRunSystem
{
    private readonly EcsPool<PhysicsComponent> _physicsPool = world.GetPool<PhysicsComponent>();
    private readonly EcsPool<PositionComponent> _positionPool = world.GetPool<PositionComponent>();
    private readonly EcsPool<RenderComponent> _renderPool = world.GetPool<RenderComponent>();

    public void Run(IEcsSystems systems)
    {
        if (keyboardManager.IsKeyPressed(Keys.L))
        {
            var world = systems.GetWorld();
            var entity = world.NewEntity();

            var (vertexBuffer, indexBuffer) = GenerateCube();

            ref var physics = ref _physicsPool.Add(entity);
            physics.Velocity = Vector3.Up;

            ref var position = ref _positionPool.Add(entity);
            position.Position = camera.Position + camera.Forward * 2 + Vector3.Up * 2 ;

            ref var render = ref _renderPool.Add(entity);
            render.VertexBuffer = vertexBuffer;
            render.IndexBuffer = indexBuffer;

            logger.LogInformation("Created entity at {pos}", position.Position);
        }
    }

    private (VertexBuffer, IndexBuffer) GenerateCube()
    {
        var cube = new CubeGenerator(blockTypes[0]);
        var vertices = new VertexPositionTexture[cube.Vertices.Length];

        for (int vertexIndex = 0; vertexIndex < cube.Vertices.Length; vertexIndex++)
        {
            vertices[vertexIndex] = new VertexPositionTexture(
                cube.Vertices[vertexIndex], cube.Uvs[vertexIndex]);
        }

        var indexBuffer = new IndexBuffer(graphicsDevice, 
            IndexElementSize.ThirtyTwoBits, cube.Triangles.Length, BufferUsage.None);

        var vertexBuffer = new VertexBuffer(graphicsDevice,
            typeof(VertexPositionTexture), cube.Uvs.Length, BufferUsage.None);

        indexBuffer.SetData(cube.Triangles);
        vertexBuffer.SetData(vertices);

        return (vertexBuffer, indexBuffer);
    }
}
