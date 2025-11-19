namespace LearnOpenTK.Uniforms;

public abstract record UniformValue<TValue>(UniformLocation Location)
{
    public abstract void SetValue(TValue value);
}
