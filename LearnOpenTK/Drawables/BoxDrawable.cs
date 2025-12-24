using LearnOpenTK.Components;
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

public static class LightingSystemFactory
{
    public static LightingSystem Create()
    {
        var lightingSystem = new LightingSystem();

        lightingSystem.AddPointLight(new PointLight
        {
            Diffuse = new Vector3(0.8f, 0.5f, 0.5f)
        });
        lightingSystem.AddPointLight(new PointLight
        {
            Diffuse = new Vector3(0.4f, 0.7f, 0.5f),
            Position = new Vector3(1.4f, 4f, 10f),
        });

        return lightingSystem;
    }
}

public class LightingSystem
{
    public List<PointLight> PointLights { get; } = new();
    public DirectionalLight DirectionalLight { get; }

    public LightingSystem()
    {
        DirectionalLight = new DirectionalLight();
    }

    public void AddPointLight(PointLight pointLight)
    {
        PointLights.Add(pointLight);
    }

    public void UpdateShaderLighting(BasicShader basicShader)
    {
        for (int pointLightIndex = 0; pointLightIndex < PointLights.Count; pointLightIndex++)
        {
            var pointLight = PointLights[pointLightIndex];
            pointLight.Apply(basicShader, $"pointLights[{pointLightIndex}]");
        }

        DirectionalLight.Apply(basicShader, "directionalLight");
    }
}

public class LightingUpdateMeshSystem(LightingSystem lightingSystem, BasicShader basicShader, LightsourceDrawable lightSource) : IUpdatable
{
    public void Update(float elapsed)
    {
        lightingSystem.PointLights[1].Position = lightSource.Position;
        lightingSystem.UpdateShaderLighting(basicShader);
    }
}

public class BoxDrawable(BasicShader shader, UltimateMeshV2ProMaxUltra mesh) : IDrawable, IUpdatable
{
    public Vector3 Position { get; set; }

    public void Update(float elapsed)
    {
    }

    public void Draw()
    {
        shader.Use();
        shader.Model.SetValue(Matrix4.CreateTranslation(Position));
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, mesh.Textures[0].Id);
        mesh.Draw();
    }
}
