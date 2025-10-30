using CubeCity.Components;
using CubeCity.GameObjects;
using CubeCity.Generators.Models;
using CubeCity.Generators.Pipelines;
using CubeCity.Systems.Utils;
using CubeCity.Threading;
using CubeCity.Tools;
using Leopotam.EcsLite;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CubeCity.Systems;

public struct ChunkRequestLoadEvent
{
    public Vector2Int Pos;
}

public struct ChunkRequestUnloadEvent
{
    public Vector2Int Pos;
}

public enum ChunkLoaderState
{
    Generating = 1,
    Loaded
}

public class ChunkLoaderInfo(ChunkLoaderState state)
{
    public EcsPackedEntity? PackedEntity;
    public ChunkLoaderState State = state;
}

public class ChunkLoaderSystem : IEcsRunSystem
{
    private readonly BackgroundProcessorPipe<GeneratorRequest, GeneratorResponse> _pipe;
    private readonly EcsWorld _world;
    private readonly ChunkGenerator _chunkGenerator;
    private readonly ILogger<ChunkLoaderSystem> _logger;
    private readonly Dictionary<Vector2Int, ChunkLoaderInfo> _chunkPosToInfo;
    private readonly EcsPool<ChunkRequestLoadEvent> _loadEvents;
    private readonly EcsPool<ChunkRequestUnloadEvent> _unloadPool;
    private readonly EcsFilter _loadFilter;
    private readonly EcsFilter _unloadFilter;

    private readonly EcsPool<ChunkComponent> _chunksPool;
    private readonly EcsPool<RenderComponent> _renderPool;
    private readonly EcsPool<PositionComponent> _positionPool;

    public ChunkLoaderSystem(EcsWorld world, ChunkGenerator chunkGenerator,
        BackgroundManager backgroundManager, ILogger<ChunkLoaderSystem> logger)
    {
        _pipe = backgroundManager.Create<GeneratorRequest, GeneratorResponse>(CreateChunk);
        _world = world;
        _chunkGenerator = chunkGenerator;
        _chunkPosToInfo = new(64);
        _logger = logger;

        _loadEvents = world.GetPool<ChunkRequestLoadEvent>();
        _unloadPool = world.GetPool<ChunkRequestUnloadEvent>();
        _loadFilter = world.Filter<ChunkRequestLoadEvent>().End();
        _unloadFilter = world.Filter<ChunkRequestUnloadEvent>().End();

        _chunksPool = world.GetPool<ChunkComponent>();
        _renderPool = world.GetPool<RenderComponent>();
        _positionPool = world.GetPool<PositionComponent>();
    }

    public void Run(IEcsSystems systems)
    {
        while (_pipe.TryPoll(out var generatorResult))
        {
            PlaceGeneratedInWorld(generatorResult);
        }
        EnqueueGeneratingRequests();
        UnloadUnusedChunks();
    }

    private void PlaceGeneratedInWorld(GeneratorResponse response)
    {
        var chunkInfo = _chunkPosToInfo[response.ChunkPos];
        var result = response.Result;

        if (chunkInfo.State is ChunkLoaderState.Loaded)
        {
            _logger.LogInformation("Skipped chunk update due state {State}", chunkInfo.State);
            // next step updating chunk when blocks updated
            result.Dispose();
            return;
        }

        var entity = _world.NewEntity();
        chunkInfo.PackedEntity = _world.PackEntity(entity);

        ref var chunk = ref _chunksPool.Add(entity);
        chunk.Blocks = result.ChunkInfo.Blocks;
        chunk.Position = response.ChunkPos;

        ref var render = ref _renderPool.Add(entity);
        render.VertexBuffer = result.VertexBuffer;
        render.IndexBuffer = result.IndexBuffer;

        ref var position = ref _positionPool.Add(entity);
        position.Position = new Vector3(
            response.ChunkPos.X * 16,
            0,
            response.ChunkPos.Y * 16);

        chunkInfo.State = ChunkLoaderState.Loaded;
    }

    private void EnqueueGeneratingRequests()
    {
        foreach (var entity in _loadFilter)
        {
            var chunkPos = _loadEvents.Get(entity).Pos;
            _loadEvents.Del(entity);

            if (_chunkPosToInfo.TryGetValue(chunkPos, out var chunkInfo))
            {
                _logger.LogError(
                    "Got LoadComponent while entry already exists. State: {State}",
                    chunkInfo.State);
                continue;
            }

            _chunkPosToInfo.Add(chunkPos, new ChunkLoaderInfo(ChunkLoaderState.Generating));
            _pipe.Enqueue(new GeneratorRequest(chunkPos));
        }
    }

    private void UnloadUnusedChunks()
    {
        foreach (var unloadEntity in _unloadFilter)
        {
            var chunkPos = _unloadPool.Get(unloadEntity).Pos;
            _unloadPool.Del(unloadEntity);

            if (!_chunkPosToInfo.TryGetValue(chunkPos, out var chunkInfo))
            {
                _logger.LogInformation(
                    "Internal entry not found for unloading chunk: {ChunkPos}", 
                    chunkPos);
                continue;
            }

            _chunkPosToInfo.Remove(chunkPos);

            if (chunkInfo.State == ChunkLoaderState.Generating)
            {
                _logger.LogInformation(
                    "Unloaded ungenerated chunk: {ChunkPos}", 
                    chunkPos);
                continue;
            }

            if (chunkInfo.State == ChunkLoaderState.Loaded)
            {
                var entity = chunkInfo.PackedEntity!.Value.Unpack(_world);
                _renderPool.Del(entity);
                _chunksPool.Del(entity);
                _positionPool.Del(entity);
            }
            else
            {
                _logger.LogError("Expected Loaded state, but got {State}: {ChunkPos}", 
                    chunkInfo.State, chunkPos);
                continue;
            }
        }
    }

    private GeneratorResponse CreateChunk(GeneratorRequest request)
    {
        var chunkMesh = _chunkGenerator.GenerateChunkMesh2(new ChunkGenerateRequest(request.Pos));
        return new GeneratorResponse(chunkMesh.Result!.Value, chunkMesh.Position);
    }

    public record GeneratorRequest(Vector2Int Pos);
    public record GeneratorResponse(ChunkGenerateResponseResult Result, Vector2Int ChunkPos);
}

public class ChunkPlayerLoaderSystem(EcsWorld world, Camera camera,
    ChunkIsRequiredChecker chunkIsRequiredChecker, ILogger<ChunkPlayerLoaderSystem> logger) : IEcsRunSystem
{
    private readonly EcsPool<ChunkRequestLoadEvent> _chunkLoadPool = world.GetPool<ChunkRequestLoadEvent>();
    private readonly EcsPool<ChunkRequestUnloadEvent> _chunkUnloadPool = world.GetPool<ChunkRequestUnloadEvent>();
    private readonly HashSet<Vector2Int> _loadedChunks = new(128);

    private Vector2Int _playerChunkPos = new(int.MinValue, int.MinValue);

    public void Run(IEcsSystems systems)
    {
        var playerChunkPos = BlocksTools.GetChunkPosByWorld(camera.Position);

        if (playerChunkPos != _playerChunkPos)
        {
            _playerChunkPos = playerChunkPos;
            chunkIsRequiredChecker.Update(playerChunkPos);
            EnsureChunks();
        }
    }

    private void EnsureChunks()
    {
        int loadCount = LoadNewChunks();
        int unloadCount = UnloadUnusedChunks();

        if (loadCount != 0 || unloadCount != 0)
        {
            logger.LogInformation(
                "Fired {LoadCount} load and {UnloadCount} unload components",
                loadCount, unloadCount);
        }
    }

    private int LoadNewChunks()
    {
        int count = 0;

        for (int x = -chunkIsRequiredChecker.LoadRange; x < chunkIsRequiredChecker.LoadRange; x++)
        {
            for (int z = -chunkIsRequiredChecker.LoadRange; z < chunkIsRequiredChecker.LoadRange; z++)
            {
                var chunkPos = _playerChunkPos + new Vector2Int(x, z);

                if (!_loadedChunks.Contains(chunkPos))
                {
                    _loadedChunks.Add(chunkPos);
                    var entity = world.NewEntity();
                    ref var request = ref _chunkLoadPool.Add(entity);
                    request.Pos = chunkPos;
                    count++;
                }
            }
        }

        return count;
    }

    private int UnloadUnusedChunks()
    {
        int count = 0;

        foreach (var position in _loadedChunks)
        {
            if (chunkIsRequiredChecker.IsForDelete(position))
            {
                var entity = world.NewEntity();
                ref var unloadRequest = ref _chunkUnloadPool.Add(entity);
                unloadRequest.Pos = position;
                _loadedChunks.Remove(position);
            }
        }

        return count;
    }
}

// капец мне все это дело не нравится надо переписать красиво как нить
public class ChunkGeneratorSystem(EcsWorld world, Camera camera, int size, 
    ChunkIsRequiredChecker chunkIsRequiredChecker, 
    ChunkGenerator chunkGenerator) : IEcsRunSystem
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
