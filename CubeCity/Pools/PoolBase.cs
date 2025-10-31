using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;

namespace CubeCity.Pools;

public static class TexturePositionVerticesExtensions
{
    public static (VertexBuffer, IndexBuffer) ToBuffers(
        this TexturePositionVertices mesh, GraphicsDevice graphicsDevice)
    {
        var indexBuffer = new IndexBuffer(graphicsDevice,
            IndexElementSize.ThirtyTwoBits, mesh.TrianglesSize, BufferUsage.None);

        indexBuffer.SetData(mesh.InternalTriangles, 0, mesh.TrianglesSize);

        var vertexBuffer = new VertexBuffer(graphicsDevice,
            typeof(VertexPositionTexture), mesh.TextureSize, BufferUsage.None);

        vertexBuffer.SetData(mesh.InternalTexture, 0, mesh.TextureSize);

        return (vertexBuffer, indexBuffer);
    }
}

public sealed class TexturePositionVerticesMemoryPool : PoolBase<TexturePositionVertices>
{
    public static readonly TexturePositionVerticesMemoryPool Instance = new();
    
    protected override TexturePositionVertices CreateMemory(int size, int originalSize) =>
        new(new VertexPositionTexture[size], new int[size / 4 * 6], originalSize, originalSize / 4 * 6);

    protected override int GetShardedKey(int value) =>
        value - value % 1024 + 1024;

    protected override int GetShardedKey(TexturePositionVertices memory) =>
        memory.InternalTexture.Length;
}

public struct TexturePositionVertices
{
    public Memory<VertexPositionTexture> Texture => InternalTexture.AsMemory(0, TextureSize);
    public Memory<int> Triangles => InternalTriangles.AsMemory(0, TrianglesSize);

    public VertexPositionTexture[] InternalTexture { get; }
    public int[] InternalTriangles { get; }
    public int TextureSize { get; }
    public int TrianglesSize { get; }

    public TexturePositionVertices(VertexPositionTexture[] internalTexture, 
        int[] internalTriangles, int textureSize, int trianglesSize)
    {
        InternalTexture = internalTexture;
        InternalTriangles = internalTriangles;
        TextureSize = textureSize;
        TrianglesSize = trianglesSize;
    }
}

public abstract class PoolBase<TMemory> where TMemory : struct
{
    protected ILogger? Logger { get; private set; }

    private readonly ConcurrentQueue<PooledMemory<TMemory>> _containersPool;
    private readonly ConcurrentDictionary<int, ConcurrentQueue<TMemory>> _memoryPool;
    private readonly Action<PooledMemory<TMemory>> _returnDelegateCache;

    private static readonly Func<int, ConcurrentQueue<TMemory>> _createFunc = _ => new ConcurrentQueue<TMemory>();

    protected PoolBase()
    {
        _memoryPool = new();
        _containersPool = new();
        _returnDelegateCache = Return;
    }

    public void SetupLogger(ILogger logger)
    {
        Logger = logger;
    }

    public virtual void Return(PooledMemory<TMemory> memory)
    {
        var key = GetShardedKey(memory.InternalItems);

        var queue = _memoryPool.GetOrAdd(key, _createFunc);
        queue.Enqueue(memory.InternalItems);

        _containersPool.Enqueue(memory);
    }

    public virtual PooledMemory<TMemory> Get(int size)
    {
        var key = GetShardedKey(size);
        var container = CreateOrGetPooledMemory();
        
        if (_memoryPool.TryGetValue(key, out var queue) && queue.TryDequeue(out var value))
        {
            return container.Initialize(value);
        }

        return container.Initialize(CreateMemory(key, size));
    }
    
    private PooledMemory<TMemory> CreateOrGetPooledMemory() => 
        _containersPool.TryDequeue(out var container) 
            ? container 
            : new PooledMemory<TMemory>(default, _returnDelegateCache);

    protected abstract TMemory CreateMemory(int size, int originalSize);
    protected abstract int GetShardedKey(int value);
    protected abstract int GetShardedKey(TMemory memory);
}

public class PooledMemory<TMemory> where TMemory : struct
{
    private int _memoryUsersCount;
    private TMemory _items;

    private readonly Action<PooledMemory<TMemory>> _returnAction;

    public TMemory InternalItems => _items;
    
    public TMemory Items
    {
        get
        {
            if (_memoryUsersCount < 1)
                Throw();

            return _items;
        }
    }

    public PooledMemory(TMemory items, Action<PooledMemory<TMemory>> returnAction)
    {
        _returnAction = returnAction;
        Initialize(items);
    }

    public PooledMemory<TMemory> Initialize(TMemory items)
    {
        _items = items;
        _memoryUsersCount = 1;
        return this;
    }

    public void AddMemoryUser()
    {
        _memoryUsersCount++;
    }

    public bool RemoveMemoryUser()
    {
        if (--_memoryUsersCount < 1)
        {
            if (_memoryUsersCount == 0)
            {
                _returnAction.Invoke(this);
                return true;
            }

            Throw();
        }

        return false;
    }

    private static void Throw()
    {
        throw new InvalidOperationException();
    }
}