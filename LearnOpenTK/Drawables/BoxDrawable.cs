using LearnOpenTK.Components;
using LearnOpenTK.Vaos;
using OpenTK.Mathematics;

namespace LearnOpenTK.Drawables;

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
