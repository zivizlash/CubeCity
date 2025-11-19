using LearnOpenTK.Uniforms;
using OpenTK.Graphics.OpenGL4;

namespace LearnOpenTK;

public class BasicShader : Shader
{
    public UniformMatrix4 Model { get; }
    public UniformMatrix4 Transform { get; }

    public BasicShader(string name) : base(name)
    {
        Model = new UniformMatrix4(GetUniformUnsafe("model"));
        Transform = new UniformMatrix4(GetUniformUnsafe("transform"));
    }
}

public class Shader : IDisposable
{
    public string Name { get; }

    private readonly int _vertShader;
    private readonly int _fragShader;
    private readonly int _handle;

    private readonly Dictionary<string, int> _nameToLocation = [];

    public Shader(string name)
    {
        Name = name;

        var vertSource = File.ReadAllText($"Shaders\\{name}.vert");
        var fragSource = File.ReadAllText($"Shaders\\{name}.frag");

        _vertShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(_vertShader, vertSource);
        CompileShader(_vertShader);

        _fragShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(_fragShader, fragSource);
        CompileShader(_fragShader);

        _handle = GL.CreateProgram();

        GL.AttachShader(_handle, _vertShader);
        GL.AttachShader(_handle, _fragShader);

        GL.LinkProgram(_handle);

        GL.DeleteShader(_vertShader);
        GL.DeleteShader(_fragShader);

        GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out var success);

        if (success == 0)
        {
            string infoLog = GL.GetProgramInfoLog(_handle);
            throw new Exception($"Error while linking program: {infoLog}");
        }
    }

    private static void CompileShader(int shader)
    {
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);

        if (success == 0)
        {
            string infoLog = GL.GetShaderInfoLog(shader); 
            throw new Exception($"Shader compilation error: {infoLog}");
        }
    }

    public int GetAttribLocation(string attribName)
    {
        return GL.GetAttribLocation(_handle, attribName);
    }

    public UniformLocation GetUniformUnsafe(string name)
    {
        if (_nameToLocation.TryGetValue(name, out var location))
        {
            return new UniformLocation(name, location);
        }

        location = GL.GetUniformLocation(_handle, name);

        _nameToLocation[name] = location;
        return new UniformLocation(name, location);
    }

    public UniformLocation GetUniform(string name)
    {
        if (_nameToLocation.TryGetValue(name, out var location))
        {
            return new UniformLocation(name, location);
        }

        location = GL.GetUniformLocation(_handle, name);

        if (location == -1)
        {
            throw new Exception($"Uniform {name} location not found");
        }

        _nameToLocation[name] = location;
        return new UniformLocation(name, location);
    }

    public void Use()
    {
        CheckDisposed();
        GL.UseProgram(_handle);
    }

    public void Unbind()
    {
        CheckDisposed();
        GL.UseProgram(0);
    }

    private bool _disposed = false;

    private void CheckDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(Shader), $"Shader with name {Name} disposed");
        }
    }

    ~Shader()
    {
        if (!_disposed)
        {
            Console.WriteLine($"GPU resource leak for shader {Name}");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            GL.DeleteProgram(_handle);
            GC.SuppressFinalize(this);
        }
    }
}
