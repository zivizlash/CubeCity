using LearnOpenTK.Components;
using OpenTK.Mathematics;

namespace LearnOpenTK;

public class LightsourceDrawable(IVertexArrayObject vao, BasicShader shader)
    : DrawableObject(vao, null)
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
}

public class BoxDrawable(IVertexArrayObject vao, Texture2D? texture, BasicShader shader) 
    : DrawableObject(vao, texture), IUpdatable
{
    public Vector3 Position { get; set; }

    private float _time;

    public void Update(float elapsed)
    {
        _time += elapsed;
    }

    public override void Draw()
    {
        shader.Use();
        shader.Model.SetValue(Matrix4.CreateRotationX(MathF.Sin(_time) * 3)
            * Matrix4.CreateRotationY(MathF.Cos(_time) * 3)
            * Matrix4.CreateTranslation(Position));

        DrawInternal();
    }
}
