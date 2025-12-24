using LearnOpenTK.ShaderUniforms;

namespace LearnOpenTK.Drawables;

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
