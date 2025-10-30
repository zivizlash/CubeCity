using CubeCity.Components;
using CubeCity.Services;
using Leopotam.EcsLite;

namespace CubeCity.Systems;

public class PhysicsSystem(EcsWorld world, ITime time) : IEcsRunSystem
{
    private readonly EcsPool<PhysicsComponent> _physicsPool = world.GetPool<PhysicsComponent>();
    private readonly EcsPool<PositionComponent> _positionPool = world.GetPool<PositionComponent>();
    private readonly EcsFilter _physicsFilter = world.Filter<PhysicsComponent>().Inc<PositionComponent>().End();

    private const float _gravity = 0.27f;
    private const float _ground = 10f;
    private const float _reduce = 0.999f;

    public void Run(IEcsSystems systems)
    {
        var delta = time.Delta;

        foreach (var entity in _physicsFilter)
        {
            ref var physics = ref _physicsPool.Get(entity);
            ref var position = ref _positionPool.Get(entity);

            physics.Velocity.Y -= _gravity * delta;
            physics.Velocity.Y -= physics.Velocity.Y * _reduce * delta;

            position.Position.Y += physics.Velocity.Y;

            if (position.Position.Y < _ground)
            {
                position.Position.Y = _ground;
                physics.Velocity.Y = 0;
            }
        }
    }
}
