using CubeCity.Generators.Algs;
using CubeCity.Tools;
using System;

namespace CubeCity.Generators.Chunks;

public class PerlinChunkBlocksGenerator : IChunkBlocksGenerator
{
    private readonly PerlinNoise2D _perlinNoise;

    public PerlinChunkBlocksGenerator(PerlinNoise2D perlinNoise)
    {
        _perlinNoise = perlinNoise;
    }

    public void Generate(Vector2Int chunkPos, ushort[,,] blocks)
    {
        for (int x = 0; x < 16; x++)
        {
            for (int z = 0; z < 16; z++)
            {
                var height = Math.Abs((int)MathF.Round(_perlinNoise.Noise(
                    (chunkPos.X * 16 + x) * 0.04f,
                    (chunkPos.Y * 16 + z) * 0.04f,
                    16, 0.1f) * 48));

                height = Math.Clamp(Math.Max(height, 1), 1, Math.Max(height, 1));
                var intHeight = Math.Max((int)MathF.Round(height), 6);

                for (int y = 0; y < intHeight - 1; y++)
                {
                    var blockType = y < 6 ? (ushort)6 : (ushort)1;
                    blocks[x, y, z] = blockType;
                }

                if (intHeight > 15)
                    blocks[x, intHeight - 1, z] = 2;
            }
        }
    }
}
