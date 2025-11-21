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

        var boxFactory = new BoxFactory();
        var random = new Random(444);

        var world = new World();

        var lightSource = new LightsourceFactory().Create(shaders);

        world.Add(camera);
        world.Add(lightSource);
        world.Add(Enumerable.Repeat(0, 10).Select(_ => RandomizePos(boxFactory.Create(camera, shaders, lightSource), random)));

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
