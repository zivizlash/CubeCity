using OpenTK.Mathematics;

namespace LearnOpenTK;

public class Shaders
{
    public readonly BasicShader Lightsource;
    public readonly BasicShader Basic;

    public readonly Vector3 LightPos = new Vector3(0, 5, -10);

    public Shaders()
    {
        Lightsource = new BasicShader("light_source");
        Basic = new BasicShader("shader5");
    }
}
