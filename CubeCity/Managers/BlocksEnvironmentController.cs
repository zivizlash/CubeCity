using CubeCity.GameObjects;
using CubeCity.Tools;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CubeCity.Managers;

public class BlocksEnvironmentController
{
    private readonly ChunkManager _chunkManager;
    private readonly Dictionary<Vector2Int, Chunk> _chunks;
    private readonly int _size;

    private Vector2Int _previousChunkPos;

    public BlocksEnvironmentController(ChunkManager chunkManager, Vector3 playerPos, int size)
    {
        _size = size;
        _chunkManager = chunkManager;
        _chunks = new Dictionary<Vector2Int, Chunk>(new Vector2IntEqualityComparer());
        _previousChunkPos = BlocksTools.GetChunkPosByWorld(playerPos);
    }

    public void ForceChunkGenerate(Vector3 playerPos)
    {
        ForceUpdateInternal(BlocksTools.GetChunkPosByWorld(playerPos));
    }

    public void UpdatePlayerPosition(Vector3 playerPos)
    {
        var chunkPos = BlocksTools.GetChunkPosByWorld(playerPos);

        if (chunkPos == _previousChunkPos)
            return;

        ForceUpdateInternal(chunkPos);
    }

    private void ForceUpdateInternal(Vector2Int chunkPos)
    {
        _previousChunkPos = chunkPos;

        var halfSize = _size / 2;

        for (int x = -halfSize; x < halfSize; x++)
        {
            for (int z = -halfSize; z < halfSize; z++)
            {
                var deltaChunkPos = chunkPos + new Vector2Int(x, z);

                if (!_chunks.ContainsKey(deltaChunkPos))
                {
                    _chunks.Add(deltaChunkPos, _chunkManager.GenerateChunk(deltaChunkPos));
                }
            }
        }
        
        foreach (var chunk in _chunks.Values)
        {
            if (!BlocksTools.IsInRange(chunkPos, chunk.Position, _size + 4))
            {
                if (chunk.IsInWorld)
                {
                    _chunkManager.RemoveChunk(chunk.Position);
                    _chunks.Remove(chunk.Position);
                }
            }
        }
    }

    internal void RegenerateCurrentChunk(Vector3 playerPos)
    {
        var chunkPos = BlocksTools.GetChunkPosByWorld(playerPos);

        if (_chunks.TryGetValue(chunkPos, out var chunk) && chunk.IsInWorld)
        {
            _chunkManager.RemoveChunk(chunk.Position);
            _chunks.Remove(chunk.Position);
        }

        _chunks.Add(chunkPos, _chunkManager.GenerateChunk(chunkPos));
    }
}
