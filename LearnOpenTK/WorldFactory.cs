using LearnOpenTK.Components;
using LearnOpenTK.Drawables;
using LearnOpenTK.Vaos;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace LearnOpenTK;

public static class VerticesData
{
    private static readonly float[] _vertices =
    [
        -0.5f, -0.5f, -0.5f, 
         0.5f, -0.5f, -0.5f, 
         0.5f,  0.5f, -0.5f, 
         0.5f,  0.5f, -0.5f, 
        -0.5f,  0.5f, -0.5f, 
        -0.5f, -0.5f, -0.5f, 

        -0.5f, -0.5f,  0.5f, 
         0.5f, -0.5f,  0.5f, 
         0.5f,  0.5f,  0.5f, 
         0.5f,  0.5f,  0.5f, 
        -0.5f,  0.5f,  0.5f, 
        -0.5f, -0.5f,  0.5f, 

        -0.5f,  0.5f,  0.5f, 
        -0.5f,  0.5f, -0.5f, 
        -0.5f, -0.5f, -0.5f, 
        -0.5f, -0.5f, -0.5f, 
        -0.5f, -0.5f,  0.5f, 
        -0.5f,  0.5f,  0.5f, 

         0.5f,  0.5f,  0.5f, 
         0.5f,  0.5f, -0.5f, 
         0.5f, -0.5f, -0.5f, 
         0.5f, -0.5f, -0.5f, 
         0.5f, -0.5f,  0.5f, 
         0.5f,  0.5f,  0.5f, 

        -0.5f, -0.5f, -0.5f, 
         0.5f, -0.5f, -0.5f, 
         0.5f, -0.5f,  0.5f, 
         0.5f, -0.5f,  0.5f, 
        -0.5f, -0.5f,  0.5f, 
        -0.5f, -0.5f, -0.5f, 

        -0.5f,  0.5f, -0.5f, 
         0.5f,  0.5f, -0.5f, 
         0.5f,  0.5f,  0.5f, 
         0.5f,  0.5f,  0.5f, 
        -0.5f,  0.5f,  0.5f, 
        -0.5f,  0.5f, -0.5f, 
    ];

    private static readonly float[] _uvs =
    [
         0.0f, 0.0f, 1.0f, 0.0f,
         1.0f, 1.0f, 1.0f, 1.0f,
         0.0f, 1.0f, 0.0f, 0.0f,

         0.0f, 0.0f, 1.0f, 0.0f,
         1.0f, 1.0f, 1.0f, 1.0f,
         0.0f, 1.0f, 0.0f, 0.0f,

         1.0f, 0.0f, 1.0f, 1.0f,
         0.0f, 1.0f, 0.0f, 1.0f,
         0.0f, 0.0f, 1.0f, 0.0f,

         1.0f, 0.0f, 1.0f, 1.0f,
         0.0f, 1.0f, 0.0f, 1.0f,
         0.0f, 0.0f, 1.0f, 0.0f,

         0.0f, 1.0f, 1.0f, 1.0f,
         1.0f, 0.0f, 1.0f, 0.0f,
         0.0f, 0.0f, 0.0f, 1.0f,

         0.0f, 1.0f, 1.0f, 1.0f,
         1.0f, 0.0f, 1.0f, 0.0f,
         0.0f, 0.0f, 0.0f, 1.0f
    ];

    public static float[] GetRawVertices()
    {
        return [.. _vertices];
    }

    public static float[] GetTexturedVertices()
    {
        var result = new List<float>();

        for (int verticeIndex = 0, uvIndex = 0; verticeIndex < _vertices.Length; verticeIndex += 3, uvIndex += 2)
        {
            result.Add(_vertices[verticeIndex + 0]);
            result.Add(_vertices[verticeIndex + 1]);
            result.Add(_vertices[verticeIndex + 2]);
            result.Add(_uvs[uvIndex + 0]);
            result.Add(_uvs[uvIndex + 1]);
        }

        return [.. result];
    }

    public static float[] GetColoredVertices(Vector3 color)
    {
        var result = new List<float>();

        for (int verticeIndex = 0; verticeIndex < _vertices.Length; verticeIndex += 3)
        {
            result.Add(_vertices[verticeIndex + 0]);
            result.Add(_vertices[verticeIndex + 1]);
            result.Add(_vertices[verticeIndex + 2]);
            result.Add(color.X);
            result.Add(color.Y);
            result.Add(color.Z);
        }

        return [.. result];
    }
}

public class LightsourceFactory
{
    public LightsourceDrawable Create(Shaders shaders)
    {
        var vao = new VertexArrayObject(VerticesData.GetRawVertices(), null);

        return new LightsourceDrawable(vao, shaders.Lightsource)
        { 
            Position = shaders.LightPos
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

        world.Add(camera);
        world.Add(new LightsourceFactory().Create(shaders));
        world.Add(Enumerable.Repeat(0, 10).Select(_ => RandomizePos(boxFactory.Create(camera, shaders.Basic), random)));

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
