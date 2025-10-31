using CubeCity.Components;
using CubeCity.Tools;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CubeCity.Systems.Chunks;

public class ChunkLifecycleEventsSystem(EcsWorld world) : IEcsRunSystem, IEcsPostRunSystem
{
    private readonly Dictionary<Vector2Int, EcsPackedEntity> _chunkPosToInfo = new(512);

    private readonly EcsPool<PositionComponent> _positionPool = world.GetPool<PositionComponent>();
    private readonly EcsPool<ChunkComponent> _chunkPool = world.GetPool<ChunkComponent>();

    // Events from ChunkPlayerLoaderSystem
    private readonly EcsPool<ChunkUpdateEvent> _chunkUpdateEvents = world.GetPool<ChunkUpdateEvent>();
    private readonly EcsFilter _chunkUpdateEventsFilter = world.Filter<ChunkUpdateEvent>().End();

    private readonly EcsPool<ChunkUnloadEvent> _chunkUnloadEvents = world.GetPool<ChunkUnloadEvent>();
    private readonly EcsFilter _chunkUnloadEventsFilter = world.Filter<ChunkUnloadEvent>().End();

    // Flags
    private readonly EcsPool<ChunkUpdateFlag> _chunkUpdateFlags = world.GetPool<ChunkUpdateFlag>();
    private readonly EcsFilter _chunkUpdateFlagsFilter = world.Filter<ChunkUpdateFlag>().End();

    private readonly EcsPool<ChunkUnloadFlag> _chunkUnloadFlags = world.GetPool<ChunkUnloadFlag>();
    private readonly EcsFilter _chunkUnloadFlagsFilter = world.Filter<ChunkUnloadFlag>().End();

    public void Run(IEcsSystems systems)
    {
        foreach (var updateEntity in _chunkUpdateEventsFilter)
        {
            ref var chunkBlockUpdate = ref _chunkUpdateEvents.Get(updateEntity);

            if (_chunkPosToInfo.TryGetValue(chunkBlockUpdate.ChunkPos, out var packedEntity))
            {
                var entity = packedEntity.Unpack(world);
                ref var chunk = ref _chunkPool.Get(entity);

                // todo: по факту он может участвовать в генерации меша
                // и мне нужно использовать нормальные пулы с подсчетом ссылок 
                chunk.Blocks.Dispose();
                chunk.Blocks = chunkBlockUpdate.Blocks;

                if (!_chunkUpdateFlags.Has(entity))
                {
                    _chunkUpdateFlags.Add(entity);
                }
            }
            else
            {
                var entity = world.NewEntity();
                _chunkPosToInfo.Add(chunkBlockUpdate.ChunkPos, world.PackEntity(entity));

                ref var chunk = ref _chunkPool.Add(entity);
                chunk.Blocks = chunkBlockUpdate.Blocks;
                chunk.Position = chunkBlockUpdate.ChunkPos;

                ref var position = ref _positionPool.Add(entity);
                position.Position = chunkBlockUpdate.ChunkPos.ToWorldChunkPosition();

                _chunkUpdateFlags.Add(entity);
            }

            _chunkUpdateEvents.Del(updateEntity);
        }

        foreach (var unloadEntity in _chunkUnloadEventsFilter)
        {
            var chunkPos = _chunkUnloadEvents.Get(unloadEntity).ChunkPos;
            _chunkUnloadEvents.Del(unloadEntity);
            _chunkUnloadFlags.Add(_chunkPosToInfo[chunkPos].Unpack(world));
        }
    }

    public void PostRun(IEcsSystems systems)
    {
        foreach (var entity in _chunkUpdateFlagsFilter)
        {
            _chunkUpdateFlags.Del(entity);
        }

        foreach (var entity in _chunkUnloadFlagsFilter)
        {
            ref var chunk = ref _chunkPool.Get(entity);
            _chunkPosToInfo.Remove(chunk.Position);
            world.DelEntity(entity);
        }
    }
}
