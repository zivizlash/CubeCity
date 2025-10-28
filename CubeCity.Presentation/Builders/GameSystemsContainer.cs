using Leopotam.EcsLite;

namespace CubeCity.Builders;

public class GameSystemsContainer
{
    public IEcsSystems UpdateSystems { get; }
    public IEcsSystems DrawSystems { get; }
    public EcsWorld World { get; }

    public GameSystemsContainer(IEcsSystems updateSystems, IEcsSystems drawSystems, EcsWorld world)
    {
        UpdateSystems = updateSystems;
        DrawSystems = drawSystems;
        World = world;
    }
}
