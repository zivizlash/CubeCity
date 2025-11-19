using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenTK.Uniforms;

public record UniformMatrix4(UniformLocation Location) : UniformValue<Matrix4>(Location)
{
    public override void SetValue(Matrix4 value)
    {
        GL.UniformMatrix4(Location.Location, false, ref value);
    }
}
