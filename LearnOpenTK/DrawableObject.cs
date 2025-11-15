namespace LearnOpenTK;

public class DrawableObject : IDisposable
{
    public DrawableObject(VertexArrayObject vao, Shader shader, Texture2D? texture)
    {
        _vao = vao;
        _shader = shader;
        _texture = texture;
    }

    private bool _disposed;
    private readonly VertexArrayObject _vao;
    protected readonly Shader _shader;
    private readonly Texture2D? _texture;

    public virtual void Draw(float elapsed)
    {
        DrawInternal();
    }

    protected void DrawInternal()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _texture?.Use();
        _shader.Use();
        _vao.Draw();
        _texture?.Unbind();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _vao.Dispose();
            _texture?.Dispose();
            _shader.Dispose();
        }
    }
}
