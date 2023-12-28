using Leopotam.EcsLite;
using Microsoft.Xna.Framework.Graphics;

namespace CubeCity.Components;

public struct RenderComponent : IEcsAutoReset<RenderComponent>
{
    public IndexBuffer IndexBuffer;
    public VertexBuffer VertexBuffer;

    public void AutoReset(ref RenderComponent c)
    {
        c.IndexBuffer?.Dispose();
        c.VertexBuffer?.Dispose();
    }
}
