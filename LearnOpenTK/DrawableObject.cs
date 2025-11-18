using LearnOpenTK.Components;

namespace LearnOpenTK;

public class DrawableObject(IVertexArrayObject vao, Texture2D? texture) : IDrawable, IDisposable
{
    private bool _disposed;
    
    public virtual void Draw()
    {
        DrawInternal();
    }

    protected void DrawInternal()
    {
        ThrowIfDisposed();
        texture?.Use();
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
        }
    }
}
