using LearnOpenTK.Components;
using LearnOpenTK.Drawables;
using LearnOpenTK.ShaderUniforms;
using Leopotam.EcsLite;
using OpenTK.Mathematics;

namespace LearnOpenTK.Systems;

public class LoadEntitiesSystem(EcsWorld world, Camera camera) : IEcsInitSystem
{
    private readonly EcsPool<TransformComponent> _transformPool = world.GetPool<TransformComponent>();
    private readonly EcsPool<MeshComponent> _meshPool = world.GetPool<MeshComponent>();
    private readonly EcsPool<LightsourceFlagComponent> _lightsourceFlags = world.GetPool<LightsourceFlagComponent>();
    private readonly EcsPool<MovementFlagComponent> _movementFlags = world.GetPool<MovementFlagComponent>();
    private readonly EcsPool<PointLightComponent> _pointLightPool = world.GetPool<PointLightComponent>();
    private readonly EcsPool<DirectionLightComponent> _directionLigthPool = world.GetPool<DirectionLightComponent>();

    public void Init(IEcsSystems systems)
    {
        var rnd = new Random(444);
        CreateBoxes(rnd);
        
        var pointLightEntityId = CreatePointLight(new Vector3(0.4f, 0.7f, 0.5f), new Vector3(0, 4, 0));
        _movementFlags.Add(pointLightEntityId);

        CreatePointLight(new Vector3(0.8f, 0.5f, 0.5f), new Vector3(0, 0, 0));
        CreateDirectionLight(new Vector3(0.5f, 0.5f, 0.5f));
    }

    private void CreateDirectionLight(Vector3 diffuse)
    {
        var entityId = world.NewEntity();

        ref var directionLight = ref _directionLigthPool.Add(entityId);
        directionLight.Diffuse = diffuse;
        directionLight.Strength = 1.0f;
        directionLight.Direction = Vector3.UnitX + -Vector3.UnitY;
    }

    private int CreatePointLight(Vector3 diffuse, Vector3 position)
    {
        var entityId = world.NewEntity();
        ref var pointLight = ref _pointLightPool.Add(entityId);
        pointLight.Diffuse = diffuse;
        
        ref var transform = ref _transformPool.Add(entityId);
        transform.Position = position;

        _lightsourceFlags.Add(entityId);
        return entityId;
    }

    private void CreateBoxes(Random rnd)
    {
        for (int i = 0; i < 10; i++)
        {
            var ultimateMeshV2 = BoxFactory.CreateMesh(camera);
            ultimateMeshV2.Setup();
            var entityId = world.NewEntity();

            ref var transform = ref _transformPool.Add(entityId);
            transform.Position = RandomizePos(rnd);

            ref var mesh = ref _meshPool.Add(entityId);
            mesh.Mesh = ultimateMeshV2;
        }
    }

    public Vector3 RandomizePos(Random random)
    {
        float GetRandom() => (float)((random.NextDouble() - 0.5) * 5);
        var p = new Vector3(GetRandom(), GetRandom(), GetRandom());
        Console.WriteLine($"{nameof(RandomizePos)} generated {p}");
        return p;
    }
}
