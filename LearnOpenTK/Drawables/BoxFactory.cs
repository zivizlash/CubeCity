using LearnOpenTK.Components;
using LearnOpenTK.Vaos;

namespace LearnOpenTK.Drawables;

public class BoxFactory
{
    public BoxDrawable Create(Camera camera, Shaders shaders, IHasPosition lightSourcePos)
    {
        var vertices = VerticesData.GetTextureNormalsVertices();

        var vao = new TexturedVertexArrayObject(vertices, null, 36);
        var texture = new Texture2D("texture1.png");

        return new BoxDrawable(vao, texture, shaders.Basic, lightSourcePos);
    }
}
