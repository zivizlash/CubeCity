using CubeCity.Tools;
using System;

namespace CubeCity.Systems.Utils;

public class ChunkIsRequiredChecker : IChunkIsRequiredChecker
{
    public volatile int PlayerChunkPosX;
    public volatile int PlayerChunkPosY;

    public readonly int RemoveRange;
    public readonly int LoadRange;

    public ChunkIsRequiredChecker(int removeRange, int loadRange)
    {
        RemoveRange = removeRange;
        LoadRange = loadRange;

        if (LoadRange > RemoveRange)
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    public void Update(Vector2Int chunkPlayerPos) =>
        (PlayerChunkPosX, PlayerChunkPosY) = (chunkPlayerPos.X, chunkPlayerPos.Y);

    public bool IsForDelete(Vector2Int chunkPos) =>
        Math.Abs(PlayerChunkPosX - chunkPos.X) > RemoveRange ||
            Math.Abs(PlayerChunkPosY - chunkPos.Y) > RemoveRange;

    public bool IsRequired(Vector2Int chunkPos) =>
        Math.Abs(PlayerChunkPosX - chunkPos.X) <= LoadRange &&
            Math.Abs(PlayerChunkPosY - chunkPos.Y) <= LoadRange;
}
