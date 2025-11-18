using LearnOpenTK.Components;
using OpenTK.Windowing.Desktop;

namespace LearnOpenTK;

public class LightsourceFactory
{
    public LightsourceDrawable Create()
    {
        return new LightsourceDrawable(new ColoredVertexArrayObject());
    }
}

public class WorldFactory
{
    public (World, Camera) Create(GameWindow gameWindow, Shaders shaders)
    {
        var camera = new Camera(gameWindow);

        var factory = new BoxFactory();
        var random = new Random(444);

        var world = new World();
        world.Add(camera);
        world.Add(new LightsourceDrawable())
        world.Add(Enumerable.Repeat(0, 10).Select(_ => RandomizePos(factory.Create(camera, shaders.Basic), random)));

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
