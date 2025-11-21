using LearnOpenTK.Components;
using LearnOpenTK.Uniforms;
using LearnOpenTK.Vaos;
using OpenTK.Mathematics;

namespace LearnOpenTK.Drawables;

public class BoxDrawable(IVertexArrayObject vao, Texture2D? texture, BasicShader shader, IHasPosition lightSourcePos) 
    : DrawableObject(vao, texture), IUpdatable
{
    public Vector3 Position { get; set; }

    private float _time;

    private readonly UniformVector3 _lightColor = new(shader.GetUniform("lightColor"));
    private readonly UniformVector3 _lightPos = new(shader.GetUniform("lightPos"));

    public void Update(float elapsed)
    {
        _time += elapsed;
    }

    public override void Draw()
    {
        shader.Use();

        // Matrix4.CreateRotationX(MathF.Sin(_time) * 3) *Matrix4.CreateRotationY(MathF.Cos(_time) * 3) *
        shader.Model.SetValue(Matrix4.CreateTranslation(Position));

        _lightPos.SetValue(lightSourcePos.Position);
        _lightColor.SetValue(new Vector3(0.8f, 0.5f, 0.5f));

        DrawInternal();
    }
}
