using Leopotam.EcsLite;

namespace CubeCity.Builders;

public record GameSystemsContainer(IEcsSystems UpdateSystems, IEcsSystems DrawSystems, EcsWorld World);
