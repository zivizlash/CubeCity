using CubeCity.Tools;

namespace CubeCity.Models;

public readonly struct ChunkInfo
{
    public Pooled<ushort[,,]> Blocks { get; }

    public ChunkInfo(Pooled<ushort[,,]> blocks)
    {
        Blocks = blocks;
    }
}
