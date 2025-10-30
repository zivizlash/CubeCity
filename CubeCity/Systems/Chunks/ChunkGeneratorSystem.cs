using CubeCity.Components;
using CubeCity.Generators.Models;
using CubeCity.Generators.Pipelines;
using CubeCity.Systems.Chunks.Models;
using CubeCity.Threading;
using CubeCity.Tools;
using Leopotam.EcsLite;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CubeCity.Systems.Chunks;

public class ChunkGeneratorSystem : IEcsRunSystem
{
    private readonly IProcessorPipe<GeneratorRequest, GeneratorResponse> _pipe;
    private readonly ChunkGenerator _chunkGenerator;
    private readonly ILogger<ChunkGeneratorSystem> _logger;
    private readonly Dictionary<Vector2Int, ChunkLoaderInfo> _chunkPosToInfo;

    private readonly EcsWorld _world;

    private readonly EcsPool<ChunkRequestLoadEvent> _loadEvents;
    private readonly EcsPool<ChunkRequestUnloadEvent> _unloadPool;
    private readonly EcsFilter _loadFilter;
    private readonly EcsFilter _unloadFilter;

    private readonly EcsPool<ChunkComponent> _chunksPool;
    private readonly EcsPool<RenderComponent> _renderPool;
    private readonly EcsPool<PositionComponent> _positionPool;

    public ChunkGeneratorSystem(EcsWorld world, ChunkGenerator chunkGenerator,
        BackgroundManager backgroundManager, ILogger<ChunkGeneratorSystem> logger)
    {
        _pipe = backgroundManager.Create<GeneratorRequest, GeneratorResponse>(CreateChunk);
        _chunkGenerator = chunkGenerator;
        _chunkPosToInfo = new(64);
        _logger = logger;

        _world = world;
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
                _world.DelEntity(entity);
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
        var chunkMesh = _chunkGenerator.GenerateChunkMesh(new ChunkGenerateRequest(request.Pos));
        return new GeneratorResponse(chunkMesh.Result!.Value, chunkMesh.Position);
    }

    public record GeneratorRequest(Vector2Int Pos);
    public record GeneratorResponse(ChunkGenerateResponseResult Result, Vector2Int ChunkPos);
}
