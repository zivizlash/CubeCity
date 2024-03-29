﻿using CubeCity.Tools;
using Leopotam.EcsLite;

namespace CubeCity.Components;

public struct GeneratingChunkComponent : IEcsAutoReset<GeneratingChunkComponent>
{
    public Vector2Int Position;

    public void AutoReset(ref GeneratingChunkComponent c)
    {
        c.Position = default;
    }
}
