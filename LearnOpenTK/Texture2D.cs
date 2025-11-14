using OpenTK.Graphics.OpenGL4;

namespace LearnOpenTK;

public class Texture2D : IDisposable
{
    public int Texture { get; }
    public string Name { get; }

    public Texture2D(string name)
    {
        Name = name;
        var byteImage = ImageLoader.LoadImageBytes(name);

        Texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Texture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, byteImage.Width, 
            byteImage.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, byteImage.Data);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Use()
    {
        GL.BindTexture(TextureTarget.Texture2D, Texture);
    }

    public void Unbind()
    {
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    private bool _disposed;

    ~Texture2D()
    {
        if (!_disposed)
        {
            Console.WriteLine($"Memory leak for texture {Name}");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            GL.DeleteTexture(Texture);
            GC.SuppressFinalize(this);
        }
    }
}
