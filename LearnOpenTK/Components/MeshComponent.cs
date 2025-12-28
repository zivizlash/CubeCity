using LearnOpenTK.Mesh;
using Leopotam.EcsLite;

namespace LearnOpenTK.Components;

public struct MeshComponent : IEcsAutoReset<MeshComponent>
{
    public UltimateMeshV2ProMaxUltra Mesh;
    public MeshShaderType Type;

    public void AutoReset(ref MeshComponent c)
    {
        c.Type = MeshShaderType.Basic;
        c.Mesh = null!;
    }
}
