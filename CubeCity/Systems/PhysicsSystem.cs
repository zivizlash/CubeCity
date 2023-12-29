using CubeCity.Components;
using CubeCity.Models;
using CubeCity.Services;
using CubeCity.Tools;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

namespace CubeCity.Systems;

public struct CubeGenerator
{
    private const int _faces = 6;
    private const int _verticesInFace = 4;
    private const int _trianglesInFace = 6;
    private const int _uvsInFace = 4;

    private int _verticesIndex = 0;
    private int _trianglesIndex = 0;
    private int _uvsIndex = 0;

    public readonly Vector3[] Vertices = new Vector3[_faces * _verticesInFace];
    public readonly int[] Triangles = new int[_faces * _trianglesInFace];
    public readonly Vector2[] Uvs = new Vector2[_faces * _uvsInFace];

    public CubeGenerator(BlockType blockType)
    {
        for (int face = 0; face < 6; face++)
        {
            Vertices[_verticesIndex + 0] = VoxelData.Verts[VoxelData.Tris[face, 0]];
            Vertices[_verticesIndex + 1] = VoxelData.Verts[VoxelData.Tris[face, 1]];
            Vertices[_verticesIndex + 2] = VoxelData.Verts[VoxelData.Tris[face, 2]];
            Vertices[_verticesIndex + 3] = VoxelData.Verts[VoxelData.Tris[face, 3]];

            AddTexture(blockType.GetTextureId(face));

            Triangles[_trianglesIndex + 0] = _verticesIndex + 0;
            Triangles[_trianglesIndex + 1] = _verticesIndex + 1;
            Triangles[_trianglesIndex + 2] = _verticesIndex + 2;
            Triangles[_trianglesIndex + 3] = _verticesIndex + 2;
            Triangles[_trianglesIndex + 4] = _verticesIndex + 1;
            Triangles[_trianglesIndex + 5] = _verticesIndex + 3;

            _trianglesIndex += 6;
            _verticesIndex += 4;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddTexture(int textureId)
    {
        const int sizeInBlocks = 4;
        const float normalizedBlock = 1f / sizeInBlocks;

        // ReSharper disable once PossibleLossOfFraction
        float y = textureId / sizeInBlocks;
        float x = textureId - y * sizeInBlocks;

        x *= normalizedBlock;
        y *= normalizedBlock;

        y = 1f - y - normalizedBlock;

        Uvs[_uvsIndex + 0] = new Vector2(x, y + normalizedBlock);
        Uvs[_uvsIndex + 1] = new Vector2(x, y);
        Uvs[_uvsIndex + 2] = new Vector2(x + normalizedBlock, y + normalizedBlock);
        Uvs[_uvsIndex + 3] = new Vector2(x + normalizedBlock, y);

        _uvsIndex += 4;
    }
}

public class PhysicsSystem : IEcsInitSystem, IEcsRunSystem
{
    private readonly ITime _time;

    private EcsPool<PhysicsComponent> _physicsPool = null!;
    private EcsPool<PositionComponent> _positionPool = null!;
    private EcsFilter _physicsFilter = null!;

    private const float _gravity = 0.27f;
    private const float _ground = 10f;
    private const float _reduce = 0.999f;

    public PhysicsSystem(ITime time)
    {
        _time = time;
    }

    public void Init(IEcsSystems systems)
    {
        var world = systems.GetWorld();

        _physicsPool = world.GetPool<PhysicsComponent>();
        _positionPool = world.GetPool<PositionComponent>();
        _physicsFilter = world.Filter<PhysicsComponent>().Inc<PositionComponent>().End();
    }

    public void Run(IEcsSystems systems)
    {
        var delta = _time.Delta;

        foreach (var entity in _physicsFilter)
        {
            ref var physics = ref _physicsPool.Get(entity);
            ref var position = ref _positionPool.Get(entity);

            physics.Velocity.Y -= _gravity * delta;
            physics.Velocity.Y -= physics.Velocity.Y * _reduce * delta;

            position.Position.Y += physics.Velocity.Y;

            if (position.Position.Y < _ground)
            {
                position.Position.Y = _ground;
                physics.Velocity.Y = 0;
            }
        }
    }
}
