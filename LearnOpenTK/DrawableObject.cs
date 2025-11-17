using LearnOpenTK.Components;

namespace LearnOpenTK;

public class DrawableObject(VertexArrayObject vao, BasicShader shader, Texture2D? texture) : IDrawable, IDisposable
{
    private bool _disposed;
    protected readonly BasicShader Shader = shader;

    public virtual void Draw()
    {
        DrawInternal();
    }

    protected void DrawInternal()
    {
        ThrowIfDisposed();
        texture?.Use();
        Shader.Use();
        vao.Draw();
        texture?.Unbind();
    }

    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            vao.Dispose();
            texture?.Dispose();
            Shader.Dispose();
        }
    }
}
