using LearnOpenTK.Components;
using LearnOpenTK.Drawables;
using LearnOpenTK.Vaos;
using OpenTK.Windowing.Desktop;

namespace LearnOpenTK;

public class LightsourceFactory
{
    public LightsourceDrawable Create(Shaders shaders)
    {
        var vao = new VertexArrayObject(VerticesData.GetRawVertices(), null);

        return new LightsourceDrawable(vao, shaders.Lightsource)
        { 
            Position = Shaders.LightPos
        };
    }
}

public class WorldFactory
{
    public (World, Camera) Create(GameWindow gameWindow, Shaders shaders)
    {
        var camera = new Camera(gameWindow);
        var random = new Random(444);
        var world = new World();

        var lightingSystem = LightingSystemFactory.Create();
        var lightSource = new LightsourceFactory().Create(shaders);

        var boxes = Enumerable.Repeat(0, 10).Select(_ => 
            RandomizePos(BoxFactory.CreateDrawable(camera, shaders), random));

        var lightingMeshUpdateSystem = new LightingUpdateMeshSystem(lightingSystem, shaders.Basic, lightSource);

        world.Add(camera);
        world.Add(lightSource);
        world.Add(lightingMeshUpdateSystem);
        world.Add(boxes);

        return (world, camera);
    }

    public BoxDrawable RandomizePos(BoxDrawable drawable, Random random)
    {
        float GetRandom() => (float)((random.NextDouble() - 0.5) * 5);
        drawable.Position = new(GetRandom(), GetRandom(), GetRandom());
        Console.WriteLine($"{nameof(RandomizePos)} generated {drawable.Position}");
        return drawable;
    }
}
