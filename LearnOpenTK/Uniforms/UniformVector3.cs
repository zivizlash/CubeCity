using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenTK.Uniforms;

public record UniformVector3(UniformLocation Location) : UniformValue<Vector3>(Location)
{
    public override void SetValue(Vector3 value)
    {
        GL.Uniform3(Location.Location, value);
    }
}
