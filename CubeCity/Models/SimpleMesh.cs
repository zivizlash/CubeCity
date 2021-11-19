using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Threading;
using CubeCity.Tools;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace CubeCity.Models
{
    public static class ChunkBlocksPool
    {
        private static readonly ConcurrentStack<ushort[,,]> _pool;

        private static int _createdArrays;
        public static int CreatedArrays => _createdArrays;

        static ChunkBlocksPool()
        {
            _pool = new ConcurrentStack<ushort[,,]>();
        }

        public static Pooled<ushort[,,]> Get(int size, int height)
        {
            if (_pool.TryPop(out var blocks))
            {
                if (blocks.GetLength(0) != size || blocks.GetLength(1) != height || blocks.GetLength(2) != size)
                    ThrowInvalidOperation();

                return new Pooled<ushort[,,]>(blocks, Pool);
            }

            Interlocked.Increment(ref _createdArrays);
            return new Pooled<ushort[,,]>(new ushort[size, height, size], Pool);
        }

        public static void Pool(ushort[,,] blocks)
        {
            var xl = blocks.GetLength(0);
            var yl = blocks.GetLength(1);
            var zl = blocks.GetLength(2);

            for (int x = 0; x < xl; x++)
            for (int y = 0; y < yl; y++)
            for (int z = 0; z < zl; z++)
                blocks[x, y, z] = 0;

            _pool.Push(blocks);
        }

        private static void ThrowInvalidOperation()
        {
            throw new InvalidOperationException();
        }
    }

    public readonly struct PooledArraySegment<T> : IDisposable
    {
        public int Length { get; }

        public T[] Array => _pooledArray.Resource;
        public Memory<T> Memory => _pooledArray.Resource.AsMemory(0, Length);
        public Span<T> Span => _pooledArray.Resource.AsSpan(0, Length);

        private readonly Pooled<T[]> _pooledArray;

        public PooledArraySegment(Pooled<T[]> array, int length)
        {
            //System.Numerics.Vector.LessThanOrEqualAny();

            // System.Numerics.Vector.
            
            Length = length;
            _pooledArray = array;
        }

        public void Dispose()
        {
            _pooledArray.Dispose();
        }
    }

    public static class ChunkBuilderPools
    {
        public static readonly ArrayPool<Vector3> VerticesPool;
        public static readonly ArrayPool<int> TrianglesPool;
        public static readonly ArrayPool<Vector2> UvsPool;

        static ChunkBuilderPools()
        {
            VerticesPool = ArrayPool<Vector3>.Create();
            UvsPool = ArrayPool<Vector2>.Create();
            TrianglesPool = ArrayPool<int>.Create();
        }
    }

    public readonly struct ChunkInfo
    {
        public SimpleMesh Mesh { get; }
        public Pooled<ushort[,,]> Blocks { get; }

        public ChunkInfo(SimpleMesh mesh, Pooled<ushort[,,]> blocks)
        {
            Mesh = mesh;
            Blocks = blocks;
        }
    }

    public readonly struct SimpleMesh
    {
        public PooledArraySegment<VertexPositionTexture> Vertices { get; }
        public PooledArraySegment<int> Triangles { get; }

        public SimpleMesh(PooledArraySegment<VertexPositionTexture> vertices, PooledArraySegment<int> triangles)
        {
            Vertices = vertices;
            Triangles = triangles;
        }
    }
}
