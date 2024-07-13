using CubeCity.Components;
using CubeCity.Services;
using Leopotam.EcsLite;

namespace CubeCity.Systems;

public class PhysicsSystem : IEcsInitSystem, IEcsRunSystem
{
    private readonly ITime _time;

    private EcsPool<PhysicsComponent> _physicsPool = null!;
    private EcsPool<PositionComponent> _positionPool = null!;
    private EcsFilter _physicsFilter = null!;

    private const float _gravity = 0.27f;
    private const float _ground = 10f;
    private const float _reduce = 0.999f;

    public PhysicsSystem(ITime time)
    {
        _time = time;
    }

    public void Init(IEcsSystems systems)
    {
        var world = systems.GetWorld();

        _physicsPool = world.GetPool<PhysicsComponent>();
        _positionPool = world.GetPool<PositionComponent>();
        _physicsFilter = world.Filter<PhysicsComponent>().Inc<PositionComponent>().End();
    }

    public void Run(IEcsSystems systems)
    {
        var delta = _time.Delta;

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
