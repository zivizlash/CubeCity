using LearnOpenTK.ShaderUniforms;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenTK.Drawables;

public class PointLight
{
    public Vector3 Position { get; set; }
    public Vector3 Diffuse { get; set; }
    public float Linear { get; set; }
    public float Quadratic { get; set; }

    public void Apply(Shader shader, string? prefix)
    {
        GL.Uniform3(shader.GetUniform(prefix, "position"), Position);
        GL.Uniform3(shader.GetUniform(prefix, "diffuse"), Diffuse);
        GL.Uniform1(shader.GetUniform(prefix, "linear"), Linear);
        GL.Uniform1(shader.GetUniform(prefix, "quadratic"), Quadratic);
    }
}
