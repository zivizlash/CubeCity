using CubeCity.Tools;
using System;

namespace CubeCity.Systems;

public record ChunkIsRequiredChecker(int RemoveRange) : IChunkIsRequiredChecker
{
    public volatile int PlayerChunkPosX;
    public volatile int PlayerChunkPosY;

    public void Update(Vector2Int chunkPlayerPos) =>
        (PlayerChunkPosX, PlayerChunkPosY) = (chunkPlayerPos.X, chunkPlayerPos.Y);

    public bool IsRequired(Vector2Int chunkPos) =>
        Math.Abs(PlayerChunkPosX - chunkPos.X) < RemoveRange &&
            Math.Abs(PlayerChunkPosY - chunkPos.Y) < RemoveRange;
}
