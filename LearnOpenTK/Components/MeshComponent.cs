using LearnOpenTK.Mesh;
using LearnOpenTK.Vaos;
using Leopotam.EcsLite;

namespace LearnOpenTK.Components;

public struct Mesh2Component : IEcsAutoReset<Mesh2Component>
{
    public IVertexArrayObject Vao;

    public void AutoReset(ref Mesh2Component c)
    {
        c.Vao = null!;
    }
}

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
