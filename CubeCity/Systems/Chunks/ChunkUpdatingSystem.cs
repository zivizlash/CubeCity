using CubeCity.Components;
using CubeCity.Tools;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CubeCity.Systems.Chunks;

public struct ChunkPhysicsUnloadEvent
{
    public Vector2Int ChunkPos;
}

public class ChunkUpdatingSystem(EcsWorld world) : IEcsRunSystem
{
    private readonly Dictionary<Vector2Int, EcsPackedEntity> _chunkPosToInfo = new(512);
    private readonly EcsPool<ChunkBlocksUpdateEvent> _blocksUpdatesEvents = world.GetPool<ChunkBlocksUpdateEvent>();
    private readonly EcsPool<ChunkUnloadEvent> _blocksUnloadEvents = world.GetPool<ChunkUnloadEvent>();
    private readonly EcsPool<PositionComponent> _positionPool = world.GetPool<PositionComponent>();
    private readonly EcsPool<ChunkComponent> _chunkPool = world.GetPool<ChunkComponent>();
    private readonly EcsPool<ChunkBlocksUpdateFlag> _chunkUpdateFlags = world.GetPool<ChunkBlocksUpdateFlag>();
    private readonly EcsPool<ChunkPhysicsUnloadEvent> _chunkPhysicsUnloadEvents = world.GetPool<ChunkPhysicsUnloadEvent>();
    private readonly EcsFilter _chunkBlockUpdatesFilter = world.Filter<ChunkBlocksUpdateEvent>().End();
    private readonly EcsFilter _chunkBlockUnloadFilter = world.Filter<ChunkUnloadEvent>().End();

    public void Run(IEcsSystems systems)
    {
        ProcessUpdates();
        ProcessUnload();
    }

    private void ProcessUnload()
    {
        foreach (var unloadEntity in _chunkBlockUnloadFilter)
        {
            ref var chunkBlockUnload = ref _blocksUnloadEvents.Get(unloadEntity);
            var entity = _chunkPosToInfo[chunkBlockUnload.ChunkPos].Unpack(world);
            _chunkPosToInfo.Remove(chunkBlockUnload.ChunkPos);

            world.DelEntity(entity);
            _blocksUnloadEvents.Del(unloadEntity);

            _chunkPhysicsUnloadEvents.Add(world.NewEntity());
        }
    }

    private void ProcessUpdates()
    {
        foreach (var updateEntity in _chunkBlockUpdatesFilter)
        {
            ref var chunkBlockUpdate = ref _blocksUpdatesEvents.Get(updateEntity);

            if (_chunkPosToInfo.TryGetValue(chunkBlockUpdate.ChunkPos, out var packedEntity))
            {
                var entity = packedEntity.Unpack(world);

                // todo: это пипяу, по факту он может участвовать в генерации меша
                // и мне нужно использовать нормальные пулы с подсчетом ссылок 
                // аааааааааааааааааааааааааааааааааааааааааааа
                ref var chunk = ref _chunkPool.Get(entity);
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
                position.Position = new Vector3(
                    chunkBlockUpdate.ChunkPos.X * 16,
                    0,
                    chunkBlockUpdate.ChunkPos.Y * 16);

                _chunkUpdateFlags.Add(entity);
            }

            _blocksUpdatesEvents.Del(updateEntity);
        }
    }
}
