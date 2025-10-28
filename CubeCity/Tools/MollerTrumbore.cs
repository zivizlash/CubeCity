using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CubeCity.Tools;

public readonly record struct ChunkTriangle(Vector3 P0, Vector3 P1, Vector3 P2);

public readonly struct ChunkMetadataFactory
{
    private readonly ushort[,,] _blocks;
    private readonly Vector3Int _ranks;
    private readonly List<ChunkTriangle> _tris;

    public ChunkMetadataFactory(ushort[,,] blocks)
    {
        _blocks = blocks;
        _ranks = GetBlockRanks(blocks);
        _tris = new List<ChunkTriangle>();
    }

    public ChunkMetadata Create()
    {
        for (int x = 0; x < _ranks.X; x++)
        {
            for (int y = 0; y < _ranks.Y; y++)
            {
                for (int z = 0; z < _ranks.Z; z++)
                {
                    if (BlockExists(x, y, z))
                    {
                        UpdateMeshData(x, y, z);
                    }
                }
            }
        }

        return new ChunkMetadata();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateMeshData(int x, int y, int z)
    {
        var pos = new Vector3Int(x, y, z);
        
        for (int face = 0; face < 6; face++)
        {
            // При face = 2 - это верх.
            if (!BlockExistsOrWorldBottom(pos + VoxelData.Faces[face]))
            {
                
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly bool BlockExistsOrWorldBottom(Vector3Int pos)
    {
        if (pos.X >= _ranks.X || pos.X < 0) return false;
        if (pos.Z >= _ranks.Z || pos.Z < 0) return false;
        if (pos.Y >= _ranks.Y || pos.Y < 0) return false;

        return _blocks[pos.X, pos.Y, pos.Z] != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly bool BlockExists(int x, int y, int z)
    {
        if (x >= _ranks.X || x < 0) return false;
        if (y >= _ranks.Y || y < 0) return false;
        if (z >= _ranks.Z || z < 0) return false;

        return _blocks[x, y, z] != 0;
    }

    private static Vector3Int GetBlockRanks(ushort[,,] blocks)
    {
        return new Vector3Int(blocks.GetLength(0), blocks.GetLength(1), blocks.GetLength(2));
    }
}

public class ChunkMetadata
{
    public ChunkMetadata()
    {
    }
}

// ReSharper disable IdentifierTypo 
public class MollerTrumbore
{
    // https://en.wikipedia.org/wiki/Möller–Trumbore_intersection_algorithm
    public float TriangleIntersection(Vector3 orig, Vector3 dir, Vector3 v0, Vector3 v1, Vector3 v2)
    {
        var e1 = v1 - v0;
        var e2 = v2 - v0;

        var pVector = Vector3.Cross(dir, e2);
        var det = Vector3.Dot(e1, pVector);

        if (det < float.Epsilon && det > -float.Epsilon)
            return 0;

        var invDet = 1 / det;
        var tVector = orig - v0;

        var u = Vector3.Dot(tVector, pVector) * invDet;

        if (u < 0 || u > 1)
            return 0;
        
        var qVector = Vector3.Cross(tVector, e1) * invDet;
        var v = Vector3.Dot(dir, qVector) * invDet;

        if (v < 0 || u + v > 1)
            return 0;

        return Vector3.Dot(e2, qVector) * invDet;
    }
}
