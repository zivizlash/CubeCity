using LearnOpenTK.Components;
using LearnOpenTK.Providers;
using Leopotam.EcsLite;

namespace LearnOpenTK.Systems;

public class CameraUpdateSystem(Camera camera, ITimeProvider timeProvider) : IEcsRunSystem
{
    public void Run(IEcsSystems systems)
    {
        camera.Update(timeProvider.Elapsed);
    }
}
