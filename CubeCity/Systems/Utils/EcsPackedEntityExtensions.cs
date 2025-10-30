using CubeCity.Systems.Utils;
using Leopotam.EcsLite;
using System;

namespace CubeCity.Systems.Utils;

public static class EcsPackedEntityExtensions
{
    public static int Unpack(this EcsPackedEntity packedEntity, EcsWorld world)
    {
        if (!packedEntity.Unpack(world, out var entity))
        {
            Throw();
        }

        return entity;
    }

    private static void Throw()
    {
        throw new InvalidOperationException();
    }
}
