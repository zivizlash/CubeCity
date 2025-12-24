using LearnOpenTK.ShaderUniforms.Uniforms;

namespace LearnOpenTK.ShaderUniforms;

public class BasicShader : Shader
{
    public UniformMatrix4 Model { get; }
    public UniformMatrix4 Transform { get; }

    public BasicShader(string name) : base(name)
    {
        Model = new UniformMatrix4(GetUniform("model"));
        Transform = new UniformMatrix4(GetUniform("transform"));
    }
}
