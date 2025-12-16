namespace LearnOpenTK.Uniforms;

public readonly record struct UniformLocation(string Name, int Location)
{
    public static implicit operator int(UniformLocation location)
    {
        return location.Location;
    }
};
