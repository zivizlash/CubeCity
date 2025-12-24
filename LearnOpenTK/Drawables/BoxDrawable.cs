using LearnOpenTK.Components;
using LearnOpenTK.Mesh;
using LearnOpenTK.ShaderUniforms;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace LearnOpenTK.Drawables;

public class BoxDrawable(BasicShader shader, UltimateMeshV2ProMaxUltra mesh) : IDrawable, IUpdatable
{
    public Vector3 Position { get; set; }

    public void Update(float elapsed)
    {
    }

    public void Draw()
    {
        shader.Use();
        shader.Model.SetValue(Matrix4.CreateTranslation(Position));
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, mesh.Textures[0].Id);
        mesh.Draw();
    }
}
