using LearnOpenTK.Components;
using LearnOpenTK.Providers;
using LearnOpenTK.ShaderUniforms;
using Leopotam.EcsLite;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenTK.Systems;

public class LightSourcePositionUpdateSystem(EcsWorld world, Shaders shaders, Camera camera, ITimeProvider timeProvider) : IEcsRunSystem
{
    private readonly EcsPool<TransformComponent> _pool = world.GetPool<TransformComponent>();
    private readonly EcsPool<PointLightComponent> _pointLightPool = world.GetPool<PointLightComponent>();
    private readonly EcsPool<MovementFlagComponent> _movementFlagPool = world.GetPool<MovementFlagComponent>();
    private readonly EcsPool<DirectionLightComponent> _directionalLight = world.GetPool<DirectionLightComponent>();

    private readonly EcsFilter _dirLightFilter = world.Filter<DirectionLightComponent>().End();

    private readonly EcsFilter _filter = world
        .Filter<TransformComponent>()
        .Inc<LightsourceFlagComponent>()
        .End();

    public void Run(IEcsSystems systems)
    {
        int idx = 0;

        foreach (var entityId in _filter)
        {
            ref var transform = ref _pool.Get(entityId);
            ref var pointLight = ref _pointLightPool.Get(entityId);

            if (_movementFlagPool.Has(entityId))
            {
                var total = timeProvider.Total;
                transform.Position = new Vector3(MathF.Sin(total) * 2, transform.Position.Y, MathF.Cos(total) * 2);
            }

            var prefix = $"pointLights[{idx++}]";

            GL.Uniform3(shaders.Basic.GetUniform($"{prefix}.position"), transform.Position);
            GL.Uniform3(shaders.Basic.GetUniform($"{prefix}.diffuse"), pointLight.Diffuse);
            GL.Uniform1(shaders.Basic.GetUniform($"{prefix}.linear"), pointLight.Linear);
            GL.Uniform1(shaders.Basic.GetUniform($"{prefix}.quadratic"), pointLight.Quadratic);
        }

        foreach (var entityId in _dirLightFilter)
        {
            ref var dirLight = ref _directionalLight.Get(entityId);

            var prefix = $"directionalLight";

            GL.Uniform3(shaders.Basic.GetUniform($"{prefix}.diffuse"), dirLight.Diffuse);
            GL.Uniform3(shaders.Basic.GetUniform($"{prefix}.direction"), dirLight.Direction);
            GL.Uniform1(shaders.Basic.GetUniform($"{prefix}.strength"), dirLight.Strength);
        }
    }
}
