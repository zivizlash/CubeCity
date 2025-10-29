using CubeCity.Tools;
using System;

namespace CubeCity.Generators.Chunks;

public class CompositeChunkBlocksGenerator : IChunkBlocksGenerator
{
    private readonly IChunkBlocksGenerator[] _chunkGenerators;
    private int _currentChunkGeneratorIndex;

    public int CurrentChunkGeneratorIndex
    {
        get => _currentChunkGeneratorIndex;
        set
        {
            if (value < 0 || value > _chunkGenerators.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _currentChunkGeneratorIndex = value;
        }
    }

    public CompositeChunkBlocksGenerator(IChunkBlocksGenerator[] chunkGenerators)
    {
        _chunkGenerators = chunkGenerators;
    }

    public void Generate(Vector2Int chunkPos, ushort[,,] blocks)
    {
        _chunkGenerators[_currentChunkGeneratorIndex].Generate(chunkPos, blocks);
    }
}
