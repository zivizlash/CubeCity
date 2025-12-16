using LearnOpenTK.Components;
using LearnOpenTK.Uniforms;
using LearnOpenTK.Vaos;
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

public class LightingContainer
{
    public List<DirectionalLight> DirectionalLights { get; } = new();
    public List<PointLight> PointLights { get; } = new();

    public LightingContainer()
    {
        DirectionalLights.Add(new DirectionalLight());
        PointLights.Add(new PointLight
        {
            Diffuse = new Vector3(0.8f, 0.5f, 0.5f)
        });
        PointLights.Add(new PointLight
        {
            Diffuse = new Vector3(0.4f, 0.7f, 0.5f),
            Position = new Vector3(1.4f, 4f, 10f),
        });
    }
}

public class BoxDrawable(IVertexArrayObject vao, Texture2D? texture, BasicShader shader, IHasPosition lightSourcePos,
    LightingContainer lightingContainer) 
    : DrawableObject(vao, texture), IUpdatable
{
    public Vector3 Position { get; set; }

    public void Update(float elapsed)
    {
    }

    public override void Draw()
    {
        shader.Use();
        shader.Model.SetValue(Matrix4.CreateTranslation(Position));

        var dirLight = lightingContainer.DirectionalLights.First();
        dirLight.Apply(shader, "directionalLight");

        var pointLight = lightingContainer.PointLights.First();

        pointLight.Position = lightSourcePos.Position;
        pointLight.Apply(shader, "light");

        DrawInternal();
    }
}
