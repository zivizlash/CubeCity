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

public struct ChunkLoadComponent
{
    public Vector2Int Pos;
}

public struct ChunkUnloadComponent
{
    public Vector2Int Pos;
}

public struct ChunkLoadingComponent
{
    public Vector2Int Pos;
}

public enum ChunkLoaderState
{
    Loading = 1,
    Loaded,
    Cancelled
}

public class ChunkLoaderInfo(EcsPackedEntity packedEntity)
{
    public readonly EcsPackedEntity PackedEntity = packedEntity;
    public ChunkLoaderState State = ChunkLoaderState.Loading;
}

public class ChunkLoaderSystem : IEcsRunSystem
{
    private readonly BackgroundProcessorPipe<GeneratorRequest, GeneratorResponse> _pipe;
    private readonly EcsWorld _world;
    private readonly ChunkGenerator _chunkGenerator;
    private readonly ILogger<ChunkLoaderSystem> _logger;
    private readonly Dictionary<Vector2Int, ChunkLoaderInfo> _posToInfo;
    private readonly EcsPool<ChunkLoadComponent> _loadPool;
    private readonly EcsPool<ChunkLoadingComponent> _loadingPool;
    private readonly EcsPool<ChunkUnloadComponent> _unloadPool;
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
        _posToInfo = new(64);
        _logger = logger;

        _loadPool = world.GetPool<ChunkLoadComponent>();
        _unloadPool = world.GetPool<ChunkUnloadComponent>();
        _loadingPool = world.GetPool<ChunkLoadingComponent>();

        _loadFilter = world.Filter<ChunkLoadComponent>().End();
        _unloadFilter = world.Filter<ChunkUnloadComponent>().End();

        _chunksPool = world.GetPool<ChunkComponent>();
        _renderPool = world.GetPool<RenderComponent>();
        _positionPool = world.GetPool<PositionComponent>();
    }

    public void Run(IEcsSystems systems)
    {
        PlaceGeneratedInWorld();
        EnqueueGeneratingRequests();
        UnloadUnusedChunks();
    }

    private void PlaceGeneratedInWorld()
    {
        while (_pipe.TryPoll(out var result))
        {
            var response = result.Response;
            var chunkInfo = _posToInfo[response.Position];

            if (response.Result is null && chunkInfo.State != ChunkLoaderState.Loading)
            {
                continue;
            }

            if (response.Result.HasValue)
            {
                var chunkData = response.Result.Value;

                if (chunkInfo.State is ChunkLoaderState.Loaded or ChunkLoaderState.Cancelled)
                {
                    _logger.LogInformation("Skipped chunk update due state {State}", chunkInfo.State);
                    chunkData.Dispose();
                    continue;
                }

                var entity = chunkInfo.PackedEntity.Unpack(_world);

                ref var chunk = ref _chunksPool.Add(entity);
                chunk.Blocks = chunkData.ChunkInfo.Blocks;
                chunk.Position = response.Position;

                ref var render = ref _renderPool.Add(entity);
                render.VertexBuffer = chunkData.VertexBuffer;
                render.IndexBuffer = chunkData.IndexBuffer;

                ref var position = ref _positionPool.Add(entity);
                position.Position = new Vector3(
                    response.Position.X * 16,
                    0,
                    response.Position.Y * 16);

                chunkInfo.State = ChunkLoaderState.Loaded;
                _loadingPool.Del(entity);
            }
            else
            {
                _logger.LogInformation("Chunk generation skipped. Internal state: {State}", chunkInfo.State);

                var entity = chunkInfo.PackedEntity.Unpack(_world);

                if (chunkInfo.State == ChunkLoaderState.Loading || chunkInfo.State == ChunkLoaderState.Cancelled)
                {
                    _loadingPool.Del(entity);
                    _posToInfo.Remove(response.Position);
                    _logger.LogInformation(
                        "Deleted LoadingComponent and removed dict entry due {State}",
                        chunkInfo.State);
                    continue;
                }
            }
        }
    }

    private void EnqueueGeneratingRequests()
    {
        foreach (var entityId in _loadFilter)
        {
            var chunkPos = _loadPool.Get(entityId).Pos;

            if (_posToInfo.TryGetValue(chunkPos, out var chunkInfo))
            {
                _logger.LogError("Got LoadComponent while entry already exists. State: {State}",
                    chunkInfo.State);
                _loadPool.Del(entityId);
                continue;
            }

            ref var loading = ref _loadingPool.Add(entityId);
            loading.Pos = chunkPos;
            _loadPool.Del(entityId);

            _posToInfo.Add(chunkPos, new ChunkLoaderInfo(_world.PackEntity(entityId)));
            _pipe.Enqueue(new GeneratorRequest(chunkPos));
        }
    }

    private void UnloadUnusedChunks()
    {
        foreach (var entityId in _unloadFilter)
        {
            var chunkPos = _unloadPool.Get(entityId).Pos;
            _unloadPool.Del(entityId);

            if (!_posToInfo.TryGetValue(chunkPos, out var chunkInfo))
            {
                _logger.LogError("Internal entry not found for unloading chunk: {ChunkPos}", chunkPos);
                continue;
            }

            if (chunkInfo.State == ChunkLoaderState.Cancelled)
            {
                _logger.LogInformation(
                    "Got UnloadComponent while internal state already cancelled: {ChunkPos}", 
                    chunkPos);
                continue;
            }

            if (chunkInfo.State == ChunkLoaderState.Loading)
            {
                _logger.LogInformation("Cancelled ChunkGenerating before generating: {ChunkPos}", chunkPos);
                chunkInfo.State = ChunkLoaderState.Cancelled;
                continue;
            }

            if (chunkInfo.State != ChunkLoaderState.Loaded)
            {
                _logger.LogError("Expected Loaded state, but got {State}: {ChunkPos}", 
                    chunkInfo.State, chunkPos);
                continue;
            }

            _renderPool.Del(entityId);
            _chunksPool.Del(entityId);
            _positionPool.Del(entityId);
        }
    }

    private GeneratorResponse CreateChunk(GeneratorRequest request)
    {
        var chunkMesh = _chunkGenerator.GenerateChunkMesh(new ChunkGenerateRequest(request.Pos));
        return new GeneratorResponse(chunkMesh);
    }

    public record GeneratorRequest(Vector2Int Pos);
    public record GeneratorResponse(ChunkGenerateResponse Response);
}

public class ChunkPlayerLoaderSystem(EcsWorld world, Camera camera,
    ChunkIsRequiredChecker chunkIsRequiredChecker, ILogger<ChunkPlayerLoaderSystem> logger) : IEcsRunSystem
{
    private readonly Dictionary<Vector2Int, EcsPackedEntity> _loadingChunks = new(128);
    private readonly EcsPool<ChunkLoadComponent> _chunkLoadPool = world.GetPool<ChunkLoadComponent>();
    private readonly EcsPool<ChunkUnloadComponent> _chunkUnloadPool = world.GetPool<ChunkUnloadComponent>();

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

                if (!_loadingChunks.ContainsKey(chunkPos))
                {
                    var entity = world.NewEntity();
                    ref var request = ref _chunkLoadPool.Add(entity);
                    request.Pos = chunkPos;
                    _loadingChunks.Add(chunkPos, world.PackEntity(entity));
                    count++;
                }
            }
        }

        return count;
    }

    private int UnloadUnusedChunks()
    {
        int count = 0;

        foreach (var (position, packed) in _loadingChunks)
        {
            if (chunkIsRequiredChecker.IsForDelete(position))
            {
                // load -> loading -> loaded

                if (packed.Unpack(world, out var entity))
                {
                    count++;
                    if (_chunkLoadPool.Has(entity))
                    {
                        _chunkLoadPool.Del(entity);
                    }
                    else
                    {
                        _chunkUnloadPool.Add(entity).Pos = position;
                    }
                }
                else
                {
                    logger.LogInformation("Failed to unpack chunk: {ChunkPos}", position);
                }

                _loadingChunks.Remove(position);
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
