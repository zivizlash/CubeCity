using Leopotam.EcsLite;
using OpenTK.Mathematics;

namespace LearnOpenTK.Components;

public struct DirectionLightComponent : IEcsAutoReset<DirectionLightComponent>
{
    public Vector3 Diffuse;
    public Vector3 Direction;
    public float Strength;

    public void AutoReset(ref DirectionLightComponent c)
    {
        c.Diffuse = default;
        c.Direction = default;
        c.Strength = default;
    }
}

public struct PointLightComponent : IEcsAutoReset<PointLightComponent>
{
    public Vector3 Diffuse;
    public float Linear;
    public float Quadratic;

    public void AutoReset(ref PointLightComponent c)
    {
        c.Diffuse = default;
        c.Linear = default;
        c.Quadratic = default;
    }
}

public struct TransformComponent
{
    public Vector3 Position;
}
