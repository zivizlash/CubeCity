using CubeCity.Tools;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CubeCity.Models;

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
