using OpenTK.Mathematics;

namespace LearnOpenTK.ShaderUniforms;

public class Shaders
{
    public readonly BasicShader Lightsource;
    public readonly BasicShader Basic;

    public static readonly Vector3 LightPos = new(0, 7, 0);

    public Shaders()
    {
        Lightsource = new BasicShader("light_source");
        Basic = new BasicShader("shader5");
    }
}
