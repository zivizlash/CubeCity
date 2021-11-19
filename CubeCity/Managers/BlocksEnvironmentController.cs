using System;
using System.Collections.Generic;
using CubeCity.GameObjects;
using CubeCity.Tools;
using Microsoft.Xna.Framework;

namespace CubeCity.Managers
{
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
            _previousChunkPos = GetChunkPosByPlayerPos(playerPos);
        }

        public void ForceChunkGenerate(Vector3 playerPos)
        {
            ForceUpdateInternal(GetChunkPosByPlayerPos(playerPos));
        }

        public void UpdatePlayerPosition(Vector3 playerPos)
        {
            var chunkPos = GetChunkPosByPlayerPos(playerPos);

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
                    var chunkPosition = chunkPos + new Vector2Int(x, z);

                    if (!_chunks.ContainsKey(chunkPosition))
                    {
                        _chunks.Add(chunkPosition, _chunkManager.GenerateChunk(chunkPosition));
                    }
                }
            }
            
            foreach (var chunk in _chunks.Values)
            {
                if (!IsInRange(chunkPos, chunk.Position, _size + 4))
                {
                    if (chunk.IsInWorld)
                    {
                        _chunkManager.RemoveChunk(chunk.Position);
                        _chunks.Remove(chunk.Position);
                    }
                }
            }
        }

        public static Vector2Int GetChunkPosByPlayerPos(Vector3 playerPos)
        {
            return new Vector2Int(
                (int)Math.Round(playerPos.X, MidpointRounding.ToPositiveInfinity) / 16,
                (int)Math.Round(playerPos.Z, MidpointRounding.ToPositiveInfinity) / 16);
        }

        private static bool IsInRange(Vector2Int playerPos, Vector2Int chunkPos, int rangeSize)
        {
            return 
                Math.Abs(playerPos.X - chunkPos.X) < rangeSize &&
                Math.Abs(playerPos.Y - chunkPos.Y) < rangeSize;
        }
    }
}
