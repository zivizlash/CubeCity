using CubeCity.Components;
using CubeCity.GameObjects;
using CubeCity.Generators.Models;
using CubeCity.Generators.Pipelines;
using CubeCity.Systems.Utils;
using CubeCity.Tools;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CubeCity.Systems;

// капец мне все это дело не нравится надо переписать красиво как нить
public class ChunkGeneratorSystem(EcsWorld world, Camera camera, int size, 
    ChunkIsRequiredChecker chunkIsRequiredChecker, 
    ChunkBlockGenerator chunkGenerator) : IEcsRunSystem
{
    private readonly Dictionary<Vector2Int, EcsPackedEntity> _chunkPosToEntity = new(512);
    private readonly EcsPool<ChunkRequestComponent> _requestsPool = world.GetPool<ChunkRequestComponent>();
    private readonly EcsPool<ChunkComponent> _chunksPool = world.GetPool<ChunkComponent>();
    private readonly EcsPool<RenderComponent> _renderPool = world.GetPool<RenderComponent>();
    private readonly EcsPool<PositionComponent> _positionPool = world.GetPool<PositionComponent>();

    private Vector2Int _playerChunkPos = new(int.MinValue, int.MinValue);

    public void Run(IEcsSystems systems)
    {
        PlaceGeneratedChunks();

        var playerChunkPos = BlocksTools.GetChunkPosByWorld(camera.Position);

        if (playerChunkPos != _playerChunkPos)
        {
            _playerChunkPos = playerChunkPos;
            chunkIsRequiredChecker.Update(playerChunkPos);
            ForceUpdateInternal();
        }
    }

    private void PlaceGeneratedChunks()
    {
        while (chunkGenerator.TryGetChunk(out var chunkResponse))
        {
            PlaceChunk(chunkResponse);
        }
    }

    private void PlaceChunk(ChunkGenerateResponse chunkResponse)
    {
        if (!_chunkPosToEntity.TryGetValue(chunkResponse.Position, out var packedEntity))
        {
            chunkResponse.Result?.Dispose();
            return;
        }

        if (chunkResponse.Result is not ChunkGenerateResponseResult result)
        {
            _chunkPosToEntity.Remove(chunkResponse.Position);

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
            _chunkPosToEntity.Remove(chunkResponse.Position);
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

    private void ForceUpdateInternal()
    {
        foreach (var (position, packed) in _chunkPosToEntity)
        {
            if (!chunkIsRequiredChecker.IsRequired(position))
            {
                world.DelEntity(packed.Unpack(world));
                _chunkPosToEntity.Remove(position);
            }
        }

        for (int x = -size; x < size; x++)
        {
            for (int z = -size; z < size; z++)
            {
                var chunkPos = _playerChunkPos + new Vector2Int(x, z);

                if (!_chunkPosToEntity.ContainsKey(chunkPos))
                {
                    var entity = world.NewEntity();
                    ref var request = ref _requestsPool.Add(entity);
                    request.Position = chunkPos;

                    _chunkPosToEntity.Add(chunkPos, world.PackEntity(entity));
                    chunkGenerator.AddGenerationRequest(new ChunkGenerateRequest(chunkPos));
                }
            }
        }
    }
}
