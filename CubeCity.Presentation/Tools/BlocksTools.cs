using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using System;

namespace CubeCity.Tools;

public static class EcsSystemExtensions
{
    public static IEcsSystems InitChain(this IEcsSystems systems)
    {
        systems.Init();
        return systems;
    }
}

public static class BlocksTools
{
    public static Vector2Int GetChunkPosByWorld(Vector3 pos)
    {
        return new Vector2Int(
            (int)(pos.X / 16),
            (int)(pos.Z / 16));
    }

    public static bool IsInRange(Vector2Int playerPos, Vector2Int chunkPos, int rangeSize)
    {
        return
            Math.Abs(playerPos.X - chunkPos.X) < rangeSize &&
            Math.Abs(playerPos.Y - chunkPos.Y) < rangeSize;
    }
}
