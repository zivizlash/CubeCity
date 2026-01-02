using LearnOpenTK.Components;
using LearnOpenTK.Mesh;
using LearnOpenTK.ShaderUniforms;

namespace LearnOpenTK.Drawables;

public static class BoxFactory
{
    public static readonly Texture2D Texture = new("texture1.png");

    public static BoxDrawable CreateDrawable(Camera camera, Shaders shaders)
    {
        var mesh = CreateMesh(camera);
        mesh.Setup();
        return new BoxDrawable(shaders.Basic, mesh);
    }

    public static UltimateMeshV2ProMaxUltra CreateMesh(Camera camera)
    {
        var vertices = VerticesCubeData.GetTextureNormalsVertices2();

        var texture = new Texture
        {
            Id = Texture.Texture,
            Type = TextureType.Diffuse
        };

        var mesh = new UltimateMeshV2ProMaxUltra
        {
            Indices = null,
            Textures = [texture],
            Vertices = vertices
        };

        return mesh;
    }
}
