using LearnOpenTK.Components;
using LearnOpenTK.Drawables;
using LearnOpenTK.Mesh;
using LearnOpenTK.ShaderUniforms;
using LearnOpenTK.Vaos;
using Leopotam.EcsLite;
using OpenTK.Mathematics;

namespace LearnOpenTK.Systems;

public class LoadEntitiesSystem(EcsWorld world, Camera camera) : IEcsInitSystem
{
    private readonly EcsPool<TransformComponent> _transformPool = world.GetPool<TransformComponent>();
    private readonly EcsPool<MeshComponent> _meshPool = world.GetPool<MeshComponent>();
    private readonly EcsPool<Mesh2Component> _mesh2Pool = world.GetPool<Mesh2Component>();
    private readonly EcsPool<LightsourceFlagComponent> _lightsourceFlags = world.GetPool<LightsourceFlagComponent>();
    private readonly EcsPool<MovementFlagComponent> _movementFlags = world.GetPool<MovementFlagComponent>();
    private readonly EcsPool<PointLightComponent> _pointLightPool = world.GetPool<PointLightComponent>();
    private readonly EcsPool<DirectionLightComponent> _directionLigthPool = world.GetPool<DirectionLightComponent>();

    public void Init(IEcsSystems systems)
    {
        var rnd = new Random(444);
        CreateBoxes(rnd);
        
        var pointLightEntityId = CreatePointLight(new Vector3(0.8f, 0.8f, 0.8f), new Vector3(0, 4, 0));
        _movementFlags.Add(pointLightEntityId);

        CreatePointLight(new Vector3(1.0f, 0.5f, 0.5f), new Vector3(0, 0, 0));
        CreateDirectionLight(new Vector3(0.1f, 0.1f, 0.1f));
        CreateFloor();
    }

    private void CreateFloor()
    {
        var entityId = world.NewEntity();

        var texture = new Texture2D("texture2.jpg");

        // uvs: 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f,

        Vertex[] vertices =
        [
            new Vertex
            {
                Position = new Vector3(-1, 0, -1),
                Normal = new Vector3(0, 1, 0),
                TexCoords = new Vector2()
            },
            new Vertex
            {
                Position = new Vector3(-1, 0, -1),
                Normal = new Vector3(0, 1, 0),
                TexCoords = new Vector2()
            },
            new Vertex
            {
                Position = new Vector3(-1, 0, -1),
                Normal = new Vector3(0, 1, 0),
                TexCoords = new Vector2()
            },
            new Vertex
            {
                Position = new Vector3(-1, 0, -1),
                Normal = new Vector3(0, 1, 0),
                TexCoords = new Vector2()
            }
        ];

        var ultimateMesh = new UltimateMeshV2ProMaxUltra
        { 
            Indices = null,
            Textures = 
            [ 
                new Texture 
                {
                    Id = texture.Texture,
                    Type = TextureType.Diffuse
                }
            ],
            Vertices = vertices
        };

        ref var mesh = ref _meshPool.Add(entityId);
        mesh.Mesh = ultimateMesh;
        mesh.Type = MeshShaderType.Basic;

        ref var transform = ref _transformPool.Add(entityId);
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

        ref var mesh = ref _mesh2Pool.Add(entityId);
        mesh.Vao = new VertexArrayObject(VerticesCubeData.GetRawVertices(), null);

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
