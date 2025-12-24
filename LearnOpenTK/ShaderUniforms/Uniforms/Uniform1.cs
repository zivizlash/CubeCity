using OpenTK.Graphics.OpenGL4;

namespace LearnOpenTK.ShaderUniforms.Uniforms;

public record Uniform1(UniformLocation Location) : UniformValue<float>(Location)
{
    public override void SetValue(float value)
    {
        GL.Uniform1(Location.Location, value);
    }
}
