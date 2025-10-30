using CubeCity.GameObjects;
using CubeCity.Services;
using CubeCity.Systems.Chunks.Models;
using CubeCity.Tools;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace CubeCity.Systems.Chunks;

public class ChunkPlayerLoaderSystem(EcsWorld world, Camera camera,
    ChunkIsRequiredChecker chunkIsRequiredChecker) : IEcsRunSystem
{
    private readonly EcsPool<ChunkBlocksFetchEvent> _chunkLoadPool = world.GetPool<ChunkBlocksFetchEvent>();
    private readonly EcsPool<ChunkBlocksUnloadEvent> _chunkUnloadPool = world.GetPool<ChunkBlocksUnloadEvent>();
    private readonly HashSet<Vector2Int> _loadedChunks = new(128);

    private Vector2Int _playerChunkPos = new(int.MinValue, int.MinValue);

    public void Run(IEcsSystems systems)
    {
        var playerChunkPos = BlocksTools.GetChunkPosByWorld(camera.Position);

        if (playerChunkPos != _playerChunkPos)
        {
            _playerChunkPos = playerChunkPos;
            chunkIsRequiredChecker.Update(playerChunkPos);

            RequestNewChunks();
            RequestUnloadNotInRangeChunks();
        }
    }

    private void RequestNewChunks()
    {
        for (int x = -chunkIsRequiredChecker.LoadRange; x < chunkIsRequiredChecker.LoadRange; x++)
        {
            for (int z = -chunkIsRequiredChecker.LoadRange; z < chunkIsRequiredChecker.LoadRange; z++)
            {
                var chunkPos = _playerChunkPos + new Vector2Int(x, z);

                if (_loadedChunks.Add(chunkPos))
                {
                    var entity = world.NewEntity();
                    ref var request = ref _chunkLoadPool.Add(entity);
                    request.Pos = chunkPos;
                }
            }
        }
    }

    private void RequestUnloadNotInRangeChunks()
    {
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
    }
}
