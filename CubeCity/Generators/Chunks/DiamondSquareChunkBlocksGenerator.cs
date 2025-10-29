using CubeCity.Generators.Algs;
using CubeCity.Tools;
using System;

namespace CubeCity.Generators.Chunks;

public class DiamondSquareChunkBlocksGenerator : IChunkBlocksGenerator
{
    private readonly double[,] _sharedTerrain;
    private readonly double[,] _tempHeightsArray;

    public DiamondSquareChunkBlocksGenerator()
    {
        var diamondSquare = new DiamondSquare(16 * 64, 0.15, 0.2, 5);
        _sharedTerrain = diamondSquare.Generate();
        _tempHeightsArray = new double[16, 16];
    }

    public void Generate(Vector2Int chunkPos, ushort[,,] blocks)
    {
        for (int x = 0; x < 16; x++)
        {
            for (int z = 0; z < 16; z++)
            {
                int height;
                FillHeightsToArray(chunkPos, _tempHeightsArray);
                height = (int)Math.Round(Math.Clamp(_tempHeightsArray[x, z] * 128, 1, 128));

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

    private void FillHeightsToArray(Vector2Int position, double[,] heights)
    {
        var normalized = new Vector2Int(Math.Abs(position.X) % 64, Math.Abs(position.Y % 64));

        for (int x = normalized.X * 16, fx = 0; x < normalized.X * 16 + 16; x++, fx++)
        {
            for (int y = normalized.Y * 16, fy = 0; y < normalized.Y * 16 + 16; y++, fy++)
            {
                heights[fx, fy] = _sharedTerrain[x, y];
            }
        }
    }
}
