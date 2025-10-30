using CubeCity.Tools;
using System;

namespace CubeCity.Generators.Chunks;

public class CompositeChunkBlocksGenerator(IChunkBlocksGenerator[] chunkGenerators) : IChunkBlocksGenerator
{
    private int _currentChunkGeneratorIndex;

    public int CurrentChunkGeneratorIndex
    {
        get => _currentChunkGeneratorIndex;
        set
        {
            if (value < 0 || value > chunkGenerators.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _currentChunkGeneratorIndex = value;
        }
    }

    public void Generate(Vector2Int chunkPos, ushort[,,] blocks)
    {
        chunkGenerators[_currentChunkGeneratorIndex].Generate(chunkPos, blocks);
    }
}
