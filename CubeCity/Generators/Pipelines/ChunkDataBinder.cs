namespace CubeCity.Generators.Pipelines;

public readonly struct ChunkBlocks
{
    public ushort[,,] Blocks { get; }

    public ChunkBlocks(ushort[,,] blocks)
    {
        Blocks = blocks;
    }
}

public readonly struct NearChunksInfo
{
    public ChunkBlocks Top { get; }
    public ChunkBlocks Bottom { get; }
    public ChunkBlocks Right { get; }
    public ChunkBlocks Left { get; }
    public ChunkBlocks Center { get; }

    public NearChunksInfo(ChunkBlocks top, ChunkBlocks bottom, 
        ChunkBlocks right, ChunkBlocks left, ChunkBlocks center)
    {
        Top = top;
        Bottom = bottom;
        Right = right;
        Left = left;
        Center = center;
    }
}
