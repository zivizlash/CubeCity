﻿using System;
using CubeCity.Models;
using CubeCity.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CubeCity.Pools;

namespace CubeCity.Managers
{
    public static class StorageMeshPools
    {
        public static readonly ArrayPool<VertexPositionTexture> VertexTexturePool;
        public static readonly ArrayPool<int> TrianglesPool;

        static StorageMeshPools()
        {
            VertexTexturePool = ArrayPool<VertexPositionTexture>.Create();
            TrianglesPool = ArrayPool<int>.Create();
        }

        public static void Return(VertexPositionTexture[] arr)
        {
            VertexTexturePool.Return(arr);
        }

        public static void Return(int[] arr)
        {
            TrianglesPool.Return(arr);
        }
    }

    public struct ChunkMeshBuilder
    {
        private const int FaceCount = 6;
        private const int VerticesInFace = 4;
        private const int UvsInFace = 4;
        private const int TrianglesInFace = 6;

        private readonly BlockType[] _blockTypes;
        private readonly ushort[,,] _blocks;
        private readonly Vector3Int _blockRanks;

        private int _verticesIndex;
        private int _uvsIndex;
        private int _trianglesIndex;

        private Vector3[] _verticesBuffer;
        private Vector2[] _uvsBuffer;
        private int[] _trianglesBuffer;

        public ChunkMeshBuilder(BlockType[] blockTypes, ushort[,,] blocks) : this()
        {
            _blockTypes = blockTypes;
            _blocks = blocks;
            _blockRanks = GetBlockRanks(blocks);
        }

        public PooledMemory<TexturePositionVertices> Build()
        {            
            var tempBuffers = GraphicsGeneratorItemsPool.Instance.Get(FaceCount * _blocks.Length);

            _verticesBuffer = tempBuffers.Items.InternalVertices;
            _uvsBuffer = tempBuffers.Items.InternalTextures;
            _trianglesBuffer = tempBuffers.Items.InternalIndices;

            for (int x = 0; x < _blockRanks.X; x++)
            for (int y = 0; y < _blockRanks.Y; y++)
            for (int z = 0; z < _blockRanks.Z; z++)
                if (BlockExists(x, y, z))
                    UpdateMeshData(x, y, z);

            var pooledMemory = TexturePositionVerticesMemoryPool.Instance.Get(_trianglesIndex);
            
            var vertices = pooledMemory.InternalItems.InternalTexture; // StorageMeshPools.VertexTexturePool.Rent(_verticesIndex);
            var triangles = pooledMemory.InternalItems.InternalTriangles; //StorageMeshPools.TrianglesPool.Rent(_trianglesIndex);

            Array.Copy(_trianglesBuffer, triangles, _trianglesIndex);

            for (int vertexIndex = 0; vertexIndex < _verticesIndex; vertexIndex++)
            {
                vertices[vertexIndex] = new VertexPositionTexture(
                    _verticesBuffer[vertexIndex], _uvsBuffer[vertexIndex]);
            }

            tempBuffers.RemoveMemoryUser();
            
            // var pooledVertices = new PooledArraySegment<VertexPositionTexture>(
            //     new Pooled<VertexPositionTexture[]>(vertices, StorageMeshPools.Return), 
            //     _verticesIndex);
            //
            // var pooledTriangles = new PooledArraySegment<int>(
            //     new Pooled<int[]>(triangles, StorageMeshPools.Return), 
            //     _trianglesIndex);

            return pooledMemory;
            
            // return new SimpleMesh(pooledVertices, pooledTriangles);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateMeshData(int x, int y, int z)
        {
            var pos = new Vector3Int(x, y, z);
            var blockId = _blocks[pos.X, pos.Y, pos.Z];

            for (int face = 0; face < 6; face++)
            {
                // При face = 2 - это верх.
                if (!BlockExistsOrWorldBottom(pos + VoxelData.Faces[face]))
                {
                    _verticesBuffer[_verticesIndex + 0] = pos + VoxelData.Verts[VoxelData.Tris[face, 0]];
                    _verticesBuffer[_verticesIndex + 1] = pos + VoxelData.Verts[VoxelData.Tris[face, 1]];
                    _verticesBuffer[_verticesIndex + 2] = pos + VoxelData.Verts[VoxelData.Tris[face, 2]];
                    _verticesBuffer[_verticesIndex + 3] = pos + VoxelData.Verts[VoxelData.Tris[face, 3]];

                    AddTexture(_blockTypes[blockId].GetTextureId(face));

                    _trianglesBuffer[_trianglesIndex + 0] = _verticesIndex + 0;
                    _trianglesBuffer[_trianglesIndex + 1] = _verticesIndex + 1;
                    _trianglesBuffer[_trianglesIndex + 2] = _verticesIndex + 2;
                    _trianglesBuffer[_trianglesIndex + 3] = _verticesIndex + 2;
                    _trianglesBuffer[_trianglesIndex + 4] = _verticesIndex + 1;
                    _trianglesBuffer[_trianglesIndex + 5] = _verticesIndex + 3;

                    _trianglesIndex += 6;
                    _verticesIndex += 4;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool BlockExists(Vector3Int pos)
        {
            if (pos.X >= _blockRanks.X || pos.X < 0) return false;
            if (pos.Y >= _blockRanks.Y || pos.Y < 0) return false;
            if (pos.Z >= _blockRanks.Z || pos.Z < 0) return false;

            return _blocks[pos.X, pos.Y, pos.Z] != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool BlockExistsOrWorldBottom(Vector3Int pos)
        {
            if (pos.X >= _blockRanks.X || pos.X < 0) return false;
            if (pos.Z >= _blockRanks.Z || pos.Z < 0) return false;
            if (pos.Y >= _blockRanks.Y) return false;

            if (pos.Y < 0)
                return true;

            return _blocks[pos.X, pos.Y, pos.Z] != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool BlockExists(int x, int y, int z)
        {
            if (x >= _blockRanks.X || x < 0) return false;
            if (y >= _blockRanks.Y || y < 0) return false;
            if (z >= _blockRanks.Z || z < 0) return false;

            return _blocks[x, y, z] != 0;
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

            _uvsBuffer[_uvsIndex + 0] = new Vector2(x, y + normalizedBlock);
            _uvsBuffer[_uvsIndex + 1] = new Vector2(x, y);
            _uvsBuffer[_uvsIndex + 2] = new Vector2(x + normalizedBlock, y + normalizedBlock);
            _uvsBuffer[_uvsIndex + 3] = new Vector2(x + normalizedBlock, y);

            _uvsIndex += 4;
        }

        private static Vector3Int GetBlockRanks(ushort[,,] blocks)
        {
            return new Vector3Int(blocks.GetLength(0), blocks.GetLength(1), blocks.GetLength(2));
        }
    }
}