namespace CubeCity.Generators;

public readonly struct ChunkBindRequest
{
    public ushort[,,] Blocks { get; }
    
    public ChunkBindRequest(ushort[,,] blocks)
    {
        Blocks = blocks;
    }
}
