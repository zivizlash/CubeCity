using CubeCity.Models;
using CubeCity.Tools;
using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

namespace CubeCity.Generators.Algs;

public struct CubeGenerator
{
    private const int _faces = 6;
    private const int _verticesInFace = 4;
    private const int _trianglesInFace = 6;
    private const int _uvsInFace = 4;

    private int _verticesIndex = 0;
    private int _trianglesIndex = 0;
    private int _uvsIndex = 0;

    public readonly Vector3[] Vertices = new Vector3[_faces * _verticesInFace];
    public readonly int[] Triangles = new int[_faces * _trianglesInFace];
    public readonly Vector2[] Uvs = new Vector2[_faces * _uvsInFace];

    public CubeGenerator(BlockType blockType)
    {
        Generate(blockType);
    }

    private void Generate(BlockType blockType)
    {
        for (int face = 0; face < 6; face++)
        {
            Vertices[_verticesIndex + 0] = VoxelData.Verts[VoxelData.Tris[face, 0]];
            Vertices[_verticesIndex + 1] = VoxelData.Verts[VoxelData.Tris[face, 1]];
            Vertices[_verticesIndex + 2] = VoxelData.Verts[VoxelData.Tris[face, 2]];
            Vertices[_verticesIndex + 3] = VoxelData.Verts[VoxelData.Tris[face, 3]];

            AddTexture(blockType.GetTextureId(face));

            Triangles[_trianglesIndex + 0] = _verticesIndex + 0;
            Triangles[_trianglesIndex + 1] = _verticesIndex + 1;
            Triangles[_trianglesIndex + 2] = _verticesIndex + 2;
            Triangles[_trianglesIndex + 3] = _verticesIndex + 2;
            Triangles[_trianglesIndex + 4] = _verticesIndex + 1;
            Triangles[_trianglesIndex + 5] = _verticesIndex + 3;

            _trianglesIndex += 6;
            _verticesIndex += 4;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddTexture(int textureId)
    {
        const int sizeInBlocks = 4;
        const float normalizedBlock = 1f / sizeInBlocks;

        // ReSharper disable once PossibleLossOfFraction
        float y = textureId / sizeInBlocks;
        float x = textureId - y * sizeInBlocks;

        x *= normalizedBlock;
        y *= normalizedBlock;

        y = 1f - y - normalizedBlock;

        Uvs[_uvsIndex + 0] = new Vector2(x, y + normalizedBlock);
        Uvs[_uvsIndex + 1] = new Vector2(x, y);
        Uvs[_uvsIndex + 2] = new Vector2(x + normalizedBlock, y + normalizedBlock);
        Uvs[_uvsIndex + 3] = new Vector2(x + normalizedBlock, y);

        _uvsIndex += 4;
    }
}
