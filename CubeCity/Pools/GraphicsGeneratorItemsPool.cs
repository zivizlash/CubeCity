using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace CubeCity.Pools
{
    public class PoolBase
    {
        protected ILogger Logger { get; private set; }

        public void SetupLogger(ILogger logger)
        {
            Logger = logger;
        }

        public 
    }

    public class PooledMemory<TMemory> where TMemory : struct
    {
        private int _memoryUsersCount;
        private TMemory _items;

        private readonly Action<PooledMemory<TMemory>> _returnAction;

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

        public void Initialize(TMemory items)
        {
            _items = items;
            _memoryUsersCount = 1;
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

    public class GraphicsGeneratorItemsPool
    {
        private readonly ConcurrentDictionary<int, ConcurrentQueue<GraphicsGeneratorPooledData>> _pool;
        private readonly ConcurrentQueue<GraphicsGeneratorPooledMemory> _pooledMemory;

        private ILogger<GraphicsGeneratorItemsPool>? _logger;

        public static readonly GraphicsGeneratorItemsPool Instance = new();

        public GraphicsGeneratorItemsPool()
        {
            _pool = new();
            _pooledMemory = new();
        }

        public void SetupLogger(ILogger<GraphicsGeneratorItemsPool> logger)
        {
            _logger = logger;
        }

        public GraphicsGeneratorPooledMemory Get(int faceCount)
        {
            var faceCountKey = faceCount - faceCount % 1024 + 1024;

            var textureCount = faceCount * 4;
            var verticesCount = faceCount * 4;
            var indicesCount = faceCount * 6;

            var textureBufferSize = faceCountKey * 4;
            var verticesBufferSize = faceCountKey * 4;
            var indicesBufferSize = faceCountKey * 6;
            
            if (_pool.TryGetValue(faceCountKey, out var queue) &&
                queue.TryDequeue(out var data))
            {
                return WrapInContainer(new GraphicsGeneratorItems(
                    data.Vertices, data.Indices, data.Textures,
                    verticesCount, indicesCount, textureCount,
                    Return));
            }

            _logger?.LogInformation("Created new pooled buffer with faces {Faces}", faceCount);

            var verticesBuffer = new Vector3[verticesBufferSize];
            var textureBuffer = new Vector2[textureBufferSize];
            var indicesBuffer = new int[indicesBufferSize];

            return WrapInContainer(new GraphicsGeneratorItems(
                verticesBuffer, indicesBuffer, textureBuffer,
                verticesCount, indicesCount, textureCount,
                Return));
        }

        private GraphicsGeneratorPooledMemory WrapInContainer(GraphicsGeneratorItems items)
        {
            if (_pooledMemory.TryDequeue(out var container))
            {
                container.Initialize(items);
                return container;
            }

            return new GraphicsGeneratorPooledMemory(items);
        }

        private void Return(Vector3[] v1, int[] v2, Vector2[] v3, GraphicsGeneratorPooledMemory pooledMemory)
        {
            var faceCount = v1.Length / 4;
            var queue = _pool.GetOrAdd(faceCount, _ => new ConcurrentQueue<GraphicsGeneratorPooledData>());
            queue.Enqueue(new GraphicsGeneratorPooledData(v1, v2, v3));
        }
    }

    internal readonly struct GraphicsGeneratorPooledData
    {
        public Vector3[] Vertices { get; }
        public int[] Indices { get; }
        public Vector2[] Textures { get; }

        public GraphicsGeneratorPooledData(Vector3[] vertices, int[] indices, Vector2[] textures)
        {
            Vertices = vertices;
            Indices = indices;
            Textures = textures;
        }
    }

    public class GraphicsGeneratorPooledMemory
    {
        private int _memoryUsersCount;
        private GraphicsGeneratorItems _items;

        public GraphicsGeneratorItems Items
        {
            get
            {
                if (_memoryUsersCount < 1)
                    Throw();

                return _items;
            }
        }

        public GraphicsGeneratorPooledMemory(GraphicsGeneratorItems items)
        {
            Initialize(items);
        }

        public void Initialize(GraphicsGeneratorItems items)
        {
            _items = items;
            _memoryUsersCount = 1;
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
                    Items.Return(this);
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

    public readonly struct GraphicsGeneratorItems
    {
        public Memory<Vector3> Vertices => InternalVertices.AsMemory(0, _countVertices);
        public Memory<int> Indices => InternalIndices.AsMemory(0, _countIndices);
        public Memory<Vector2> Textures => InternalTextures.AsMemory(0, _countTextures);

        public readonly Vector3[] InternalVertices;
        public readonly int[] InternalIndices;
        public readonly Vector2[] InternalTextures;

        private readonly int _countVertices;
        private readonly int _countIndices;
        private readonly int _countTextures;

        private readonly Action<Vector3[], int[], Vector2[], GraphicsGeneratorPooledMemory> _returnPool;

        public GraphicsGeneratorItems(Vector3[] vertices, int[] indices, Vector2[] textures, 
            int countVertices, int countIndices, int countTextures,
            Action<Vector3[], int[], Vector2[], GraphicsGeneratorPooledMemory> returnPool)
        {
            _returnPool = returnPool;
            InternalVertices = vertices;
            InternalIndices = indices;
            InternalTextures = textures;
            _countVertices = countVertices;
            _countIndices = countIndices;
            _countTextures = countTextures;
        }

        public void Return(GraphicsGeneratorPooledMemory pooledMemory)
        {
            _returnPool.Invoke(InternalVertices, InternalIndices, InternalTextures, pooledMemory);
        }
    }
}
