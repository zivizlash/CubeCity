using LearnOpenTK.Components;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenTK;

public class BoxDrawable(VertexArrayObject vao, BasicShader shader, Texture2D? texture, Camera camera) : DrawableObject(vao, shader, texture)
{
    public Matrix4 Projection { get; set; }
    public Matrix4 View { get; set; }
    public Matrix4 Model { get; set; }

    public Vector3 Position { get; set; }

    private float _time;

    public BoxDrawable RandomizePos(Random random)
    {
        int GetRandom() => random.Next() % 5;
        Position = new(GetRandom(), GetRandom(), GetRandom());
        return this;
    }

    public void Update(float elapsed)
    {
        _time += elapsed;
    }

    public override void Draw()
    {
        Shader.Use();
        Shader.ProjectionView.SetValue(camera.ProjectionViewMatrix);

        //Shader.Projection.SetValue(Projection);
        //Shader.View.SetValue(View);

        Shader.Model.SetValue(Matrix4.CreateRotationX(MathF.Sin(_time))
            * Matrix4.CreateRotationZ(MathF.Cos(_time))
            * Matrix4.CreateTranslation(Position));
        
        DrawInternal();
    }
}
