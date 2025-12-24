using LearnOpenTK.Components;
using LearnOpenTK.ShaderUniforms;

namespace LearnOpenTK.Drawables;

public class LightingUpdateMeshSystem(LightingSystem lightingSystem, BasicShader basicShader, LightsourceDrawable lightSource) : IUpdatable
{
    public void Update(float elapsed)
    {
        lightingSystem.PointLights[1].Position = lightSource.Position;
        lightingSystem.UpdateShaderLighting(basicShader);
    }
}
