using LearnOpenTK.Components;
using LearnOpenTK.ShaderUniforms;
using Leopotam.EcsLite;
using OpenTK.Mathematics;

namespace LearnOpenTK.Systems;

public class DrawSystem(EcsWorld world, Shaders shaders, Camera camera) : IEcsRunSystem
{
    private readonly EcsPool<MeshComponent> _meshPool = world.GetPool<MeshComponent>();
    private readonly EcsPool<TransformComponent> _transformPool = world.GetPool<TransformComponent>();
    private readonly EcsPool<Mesh2Component> _mesh2Pool = world.GetPool<Mesh2Component>();

    private readonly EcsFilter _meshFilter = world
        .Filter<MeshComponent>()
        .Inc<TransformComponent>()
        .End();

    private readonly EcsFilter _mesh2Filter = world
        .Filter<Mesh2Component>()
        .Inc<TransformComponent>()
        .End();

    public void Run(IEcsSystems systems)
    {
        shaders.Basic.Transform.SetValue(camera.ProjectionViewMatrix);
        shaders.Lightsource.Transform.SetValue(camera.ProjectionViewMatrix);

        foreach (var entityId in _meshFilter)
        {
            ref var mesh = ref _meshPool.Get(entityId);
            ref var transform = ref _transformPool.Get(entityId);

            var shader = GetShaderByType(mesh.Type);
            shader.Use();
            shader.Model.SetValue(Matrix4.CreateTranslation(transform.Position));
            mesh.Mesh.Draw();
        }

        //foreach (var entityId in _mesh2Filter)
        //{
        //    ref var mesh2 = ref _mesh2Pool.Get(entityId);
        //    ref var transform = ref _transformPool.Get(entityId);

        //    var shader = shaders.Lightsource;

        //    shader.Use();
        //    shader.Model.SetValue(Matrix4.CreateTranslation(transform.Position));
        //    mesh2.Vao.Draw();
        //}
    }

    private BasicShader GetShaderByType(MeshShaderType type)
    {
        return type switch
        {
            MeshShaderType.Basic => shaders.Basic,
            MeshShaderType.LightSource => shaders.Lightsource,
            _ => throw new NotImplementedException()
        };
    }
}
