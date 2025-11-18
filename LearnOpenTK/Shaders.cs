namespace LearnOpenTK;

public class Shaders
{
    public readonly BasicShader Lightsource;
    public readonly BasicShader Basic;

    public Shaders()
    {
        Lightsource = new BasicShader("light_source");
        Basic = new BasicShader("shader5");
    }
}
