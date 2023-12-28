using CubeCity.Components;
using CubeCity.GameObjects;
using CubeCity.Generators;
using CubeCity.Generators.Pipelines;
using CubeCity.Tools;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CubeCity.Systems;

public class ChunkGeneratorSystem : IEcsInitSystem, IEcsRunSystem
{
    private readonly Dictionary<Vector2Int, EcsPackedEntity> _entities;
    private readonly Camera _camera;
    private readonly ChunkGenerator _chunkGenerator;
    private readonly int _size;

    private EcsPool<GeneratingChunkComponent> _requestsPool = null!;
    private EcsPool<ChunkComponent> _chunksPool = null!;
    private EcsPool<RenderComponent> _renderPool = null!;
    private EcsPool<PositionComponent> _positionPool = null!;

    private Vector2Int _previousChunkPosition;

    public ChunkGeneratorSystem(Camera camera, int size, ChunkGenerator chunkGenerator)
    {
        _camera = camera;
        _size = size;
        _chunkGenerator = chunkGenerator;
        _entities = new Dictionary<Vector2Int, EcsPackedEntity>(512);
        _previousChunkPosition = new Vector2Int(int.MinValue, int.MinValue);
    }

    public void Init(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        _requestsPool = world.GetPool<GeneratingChunkComponent>();
        _chunksPool = world.GetPool<ChunkComponent>();
        _renderPool = world.GetPool<RenderComponent>();
        _positionPool = world.GetPool<PositionComponent>();
    }

    public void Run(IEcsSystems systems)
    {
        PlaceGeneratedChunks(systems);

        var chunkPos = BlocksTools.GetChunkPosByWorld(_camera.Position);

        if (chunkPos != _previousChunkPosition)
        {
            _previousChunkPosition = chunkPos;
            ForceUpdateInternal(systems);
        }
    }

    private void PlaceGeneratedChunks(IEcsSystems systems)
    {
        var world = systems.GetWorld();

        while (_chunkGenerator.TryGetChunk(out var chunkResponse))
        {
            if (!_entities[chunkResponse.Position].Unpack(world, out var entity))
            {
                throw new InvalidOperationException("Can't unpack chunk.");
            }

            ref var chunk = ref _chunksPool.Add(entity);
            chunk.Blocks = chunkResponse.ChunkInfo.Blocks;
            chunk.Position = chunkResponse.Position;

            ref var render = ref _renderPool.Add(entity);
            render.VertexBuffer = chunkResponse.VertexBuffer;
            render.IndexBuffer = chunkResponse.IndexBuffer;

            ref var position = ref _positionPool.Add(entity);
            position.Position = new Vector3(chunkResponse.Position.X * 16, 0, chunkResponse.Position.Y * 16);

            _requestsPool.Del(entity);
        }
    }

    private void ForceUpdateInternal(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        var size = _size;

        for (int x = -size; x < size; x++)
        {
            for (int z = -size; z < size; z++)
            {
                var deltaChunkPos = _previousChunkPosition + new Vector2Int(x, z);

                if (!_entities.ContainsKey(deltaChunkPos))
                {
                    var entity = world.NewEntity();
                    ref var request = ref _requestsPool.Add(entity);
                    request.Position = deltaChunkPos;

                    _entities.Add(deltaChunkPos, world.PackEntity(entity));
                    _chunkGenerator.AddGenerationRequest(new ChunkGenerateRequest(deltaChunkPos));
                }
            }
        }

        foreach (var (position, packed) in _entities)
        {
            if (!BlocksTools.IsInRange(_previousChunkPosition, position, _size + 4))
            {
                if (packed.Unpack(world, out var entity))
                {
                    if (_renderPool.Has(entity))
                    {
                        world.DelEntity(entity);
                        _entities.Remove(position);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Chunk already removed.");
                }
            }
        }
    }
}
