namespace CubeCity.Generators.Models;

public readonly struct ChunkBindRequest
{
    public ushort[,,] Blocks { get; }

    public ChunkBindRequest(ushort[,,] blocks)
    {
        Blocks = blocks;
    }
}
