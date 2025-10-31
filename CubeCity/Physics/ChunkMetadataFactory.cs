using CubeCity.Tools;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CubeCity.Physics;

public readonly struct ChunkMetadataFactory
{
    private readonly ushort[,,] _blocks;
    private readonly Vector3 _inWorldChunkPos;
    private readonly Vector3Int _ranks;
    private readonly List<ChunkTriangle> _tris;

    public IReadOnlyList<ChunkTriangle> Triangles => _tris;

    public ChunkMetadataFactory(ushort[,,] blocks, Vector3 inWorldChunkPos)
    {
        _blocks = blocks;
        _inWorldChunkPos = inWorldChunkPos;
        _ranks = GetBlockRanks(blocks);
        _tris = new List<ChunkTriangle>(2048);
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

        return new ChunkMetadata(Triangles.ToArray());
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
                var verticeP0 = pos + VoxelData.Verts[VoxelData.Tris[face, 0]] + _inWorldChunkPos;
                var verticeP1 = pos + VoxelData.Verts[VoxelData.Tris[face, 1]] + _inWorldChunkPos;
                var verticeP2 = pos + VoxelData.Verts[VoxelData.Tris[face, 2]] + _inWorldChunkPos;
                var verticeP3 = pos + VoxelData.Verts[VoxelData.Tris[face, 3]] + _inWorldChunkPos;

                var t1 = new ChunkTriangle(verticeP0, verticeP1, verticeP2);
                var t2 = new ChunkTriangle(verticeP2, verticeP1, verticeP3);

                _tris.Add(t1);
                _tris.Add(t2);
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
