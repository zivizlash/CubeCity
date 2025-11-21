using LearnOpenTK.Components;
using LearnOpenTK.Uniforms;
using LearnOpenTK.Vaos;
using OpenTK.Mathematics;

namespace LearnOpenTK.Drawables;

public interface IHasPosition
{
    Vector3 Position { get; }
}

public class LightsourceDrawable(IVertexArrayObject vao, BasicShader shader)
    : DrawableObject(vao, null), IHasPosition, IUpdatable
{
    private readonly UniformVector3 _objectColor = new(shader.GetUniform("objectColor"));
    private readonly UniformVector3 _lightColor = new(shader.GetUniform("lightColor"));

    public required Vector3 Position { get; set; }

    public override void Draw()
    {
        shader.Use();
        shader.Model.SetValue(Matrix4.CreateTranslation(Position));
        _objectColor.SetValue(new Vector3(0.77f, 0.1f, 0.1f));
        _lightColor.SetValue(Vector3.One);
        DrawInternal();
    }

    private float _total;

    public void Update(float elapsed)
    {
        _total += elapsed;
        Position = new Vector3(MathF.Sin(_total) * 2, 4, MathF.Cos(_total) * 2);
    }
}
