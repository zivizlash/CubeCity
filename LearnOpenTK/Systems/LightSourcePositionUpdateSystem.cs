using LearnOpenTK.Components;
using LearnOpenTK.Providers;
using LearnOpenTK.ShaderUniforms;
using Leopotam.EcsLite;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenTK.Systems;

public class LightSourcePositionUpdateSystem(EcsWorld world, Shaders shaders, Camera camera, ITimeProvider timeProvider) : IEcsRunSystem
{
    private readonly EcsFilter _filter = world
        .Filter<TransformComponent>()
        .Inc<LightsourceFlagComponent>()
        .Inc<MovementFlagComponent>()
        .End();

    private readonly EcsPool<TransformComponent> _pool = world.GetPool<TransformComponent>();
    private readonly EcsPool<PointLightComponent> _pointLightPool = world.GetPool<PointLightComponent>();

    public void Run(IEcsSystems systems)
    {
        int idx = 0;

        foreach (var entityId in _filter)
        {
            ref var transform = ref _pool.Get(entityId);
            var total = timeProvider.Total;
            transform.Position = new Vector3(MathF.Sin(total) * 2, transform.Position.Y, MathF.Cos(total) * 2);

            ref var pointLight = ref _pointLightPool.Get(entityId);

            var prefix = $"pointLights[{idx++}]";

            GL.Uniform3(shaders.Basic.GetUniform($"{prefix}.position"), transform.Position);
            GL.Uniform3(shaders.Basic.GetUniform($"{prefix}.diffuse"), pointLight.Diffuse);
        }
    }
}
