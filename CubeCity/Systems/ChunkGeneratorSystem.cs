using CubeCity.Components;
using CubeCity.GameObjects;
using CubeCity.Generators.Models;
using CubeCity.Generators.Pipelines;
using CubeCity.Tools;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CubeCity.Systems;

// капец мне все это дело не нравится надо переписать красиво как нить
public class ChunkGeneratorSystem(Camera camera, int size, 
    ChunkIsRequiredChecker chunkIsRequiredChecker, 
    ChunkBlockGenerator chunkGenerator) : IEcsInitSystem, IEcsRunSystem
{
    private readonly Dictionary<Vector2Int, EcsPackedEntity> _entities = new(512);
    
    private EcsPool<GeneratingChunkComponent> _requestsPool = null!;
    private EcsPool<ChunkComponent> _chunksPool = null!;
    private EcsPool<RenderComponent> _renderPool = null!;
    private EcsPool<PositionComponent> _positionPool = null!;

    private Vector2Int _playerChunkPos = new(int.MinValue, int.MinValue);

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

        var playerChunkPos = BlocksTools.GetChunkPosByWorld(camera.Position);

        if (playerChunkPos != _playerChunkPos)
        {
            _playerChunkPos = playerChunkPos;
            chunkIsRequiredChecker.Update(playerChunkPos);
            ForceUpdateInternal(systems);
        }
    }

    private void PlaceGeneratedChunks(IEcsSystems systems)
    {
        while (chunkGenerator.TryGetChunk(out var chunkResponse))
        {
            PlaceChunk(systems, chunkResponse);
        }
    }

    private void PlaceChunk(IEcsSystems systems, ChunkGenerateResponse chunkResponse)
    {
        var world = systems.GetWorld();

        if (!_entities.TryGetValue(chunkResponse.Position, out var packedEntity))
        {
            chunkResponse.Result?.Dispose();
            return;
        }

        if (chunkResponse.Result is not ChunkGenerateResponseResult result)
        {
            _entities.Remove(chunkResponse.Position);

            if (packedEntity.Unpack(world, out var deleteEntity))
            {
                world.DelEntity(deleteEntity);
            }
            return;
        }

        var entity = packedEntity.Unpack(world);

        if (!chunkIsRequiredChecker.IsRequired(chunkResponse.Position))
        {
            result.Dispose();
            world.DelEntity(entity);
            _entities.Remove(chunkResponse.Position);
            return;
        }

        if (_chunksPool.Has(entity))
        {
            result.Dispose();
            return;
        }

        ref var chunk = ref _chunksPool.Add(entity);
        chunk.Blocks = result.ChunkInfo.Blocks;
        chunk.Position = chunkResponse.Position;

        ref var render = ref _renderPool.Add(entity);
        render.VertexBuffer = result.VertexBuffer;
        render.IndexBuffer = result.IndexBuffer;

        ref var position = ref _positionPool.Add(entity);
        position.Position = new Vector3(
            chunkResponse.Position.X * 16, 
            0, 
            chunkResponse.Position.Y * 16);

        _requestsPool.Del(entity);
    }

    private void ForceUpdateInternal(IEcsSystems systems)
    {
        var world = systems.GetWorld();

        foreach (var (position, packed) in _entities)
        {
            if (!chunkIsRequiredChecker.IsRequired(position))
            {
                world.DelEntity(packed.Unpack(world));
                _entities.Remove(position);
            }
        }

        for (int x = -size; x < size; x++)
        {
            for (int z = -size; z < size; z++)
            {
                var chunkPos = _playerChunkPos + new Vector2Int(x, z);

                if (!_entities.ContainsKey(chunkPos))
                {
                    var entity = world.NewEntity();
                    ref var request = ref _requestsPool.Add(entity);
                    request.Position = chunkPos;

                    _entities.Add(chunkPos, world.PackEntity(entity));
                    chunkGenerator.AddGenerationRequest(new ChunkGenerateRequest(chunkPos));
                }
            }
        }
    }
}
