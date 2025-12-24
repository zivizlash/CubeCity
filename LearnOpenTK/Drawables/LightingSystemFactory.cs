using OpenTK.Mathematics;

namespace LearnOpenTK.Drawables;

public static class LightingSystemFactory
{
    public static LightingSystem Create()
    {
        var lightingSystem = new LightingSystem();

        lightingSystem.AddPointLight(new PointLight
        {
            Diffuse = new Vector3(0.8f, 0.5f, 0.5f)
        });
        lightingSystem.AddPointLight(new PointLight
        {
            Diffuse = new Vector3(0.4f, 0.7f, 0.5f),
            Position = new Vector3(1.4f, 4f, 10f),
        });

        return lightingSystem;
    }
}
