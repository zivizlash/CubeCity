using LearnOpenTK.ShaderUniforms;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenTK.Drawables;

public class DirectionalLight
{
    public Vector3 Direction { get; set; }
    public Vector3 Diffuse { get; set; }
    public float Strength { get; set; }

    public DirectionalLight()
    {
        Diffuse = new Vector3(0.5f, 0.5f, 0.5f);
        Direction = Vector3.UnitX + -Vector3.UnitY;
        Strength = 1.0f;
    }

    public void Apply(Shader shader, string? prefix)
    {
        GL.Uniform3(shader.GetUniform(prefix, "diffuse"), Diffuse);
        GL.Uniform3(shader.GetUniform(prefix, "direction"), Direction);
        GL.Uniform1(shader.GetUniform(prefix, "strength"), Strength);
    }
}
