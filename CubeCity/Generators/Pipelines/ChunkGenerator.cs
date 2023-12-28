using CubeCity.Generators.Algs;
using CubeCity.Managers;
using CubeCity.Models;
using CubeCity.Tools;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

namespace CubeCity.Generators.Pipelines;

public class ChunkGenerator
{
    private readonly ConcurrentQueue<ChunkGenerateResponse> _responses;
    private readonly ActionBlock<ChunkGenerateRequest> _requests;

    private readonly PerlinNoise2D _perlinNoiseNoise;
    private readonly BlockType[] _blockTypes;
    private readonly GraphicsDevice _graphicsDevice;

    private readonly double[,] _sharedTerrain;
    private readonly double[,] _tempHeightsArray;
    
    public bool UsePerlinNoise { get; set; }

    public ChunkGenerator(PerlinNoise2D perlinNoise, BlockType[] blockTypes, 
        int generatingChunkThreads, GraphicsDevice graphicsDevice)
    {
        UsePerlinNoise = true;

        _perlinNoiseNoise = perlinNoise;
        _blockTypes = blockTypes;
        _graphicsDevice = graphicsDevice;
        _responses = new ConcurrentQueue<ChunkGenerateResponse>();

        _requests = new ActionBlock<ChunkGenerateRequest>(
            GenerateChunk, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = generatingChunkThreads,
                SingleProducerConstrained = true
            });
        
        var diamondSquare = new DiamondSquare(16 * 64, 0.15, 0.2, 5);
        _sharedTerrain = diamondSquare.Generate();
        _tempHeightsArray = new double[16, 16];
    }

    public void AddGenerationRequest(ChunkGenerateRequest request)
    {
        if (!_requests.Post(request)) throw new InvalidOperationException();
    }

    public bool TryGetChunk(out ChunkGenerateResponse response)
    {
        return _responses.TryDequeue(out response);
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

    private void GenerateChunk(ChunkGenerateRequest request)
    {
        var position = request.Position;

        var pooledBlocks = ChunkBlocksPool.Get(16, 128);
        var blocks = pooledBlocks.Resource;

        //var chunkGenType = _perlinNoiseNoise.Noise(position.X * 0.1f, position.Y * 0.1f);

        for (int x = 0; x < 16; x++)
        {
            for (int z = 0; z < 16; z++)
            {
                int height;

                if (true) //chunkGenType > 0.1f)
                {
                    height = Math.Abs((int)MathF.Round(_perlinNoiseNoise.Noise(
                        (position.X * 16 + x) * 0.04f,
                        (position.Y * 16 + z) * 0.04f,
                        16, 0.1f) * 48));
                }
                else
                {
                    // _tempHeightsArray using in several threads
                    FillHeightsToArray(position, _tempHeightsArray);
                    height = (int)Math.Round(Math.Clamp(_tempHeightsArray[x, z] * 128, 1, 128));
                }

                height = Math.Clamp(Math.Max(height, 1), 1, Math.Max(height, 1));
                var intHeight = Math.Max((int)MathF.Round(height), 6);

                for (int y = 0; y < intHeight - 1; y++)
                {
                    var blockType = y < 6 ? (ushort) 6 : (ushort) 1;
                    blocks[x, y, z] = blockType;
                }

                if (intHeight > 15)
                    blocks[x, intHeight - 1, z] = 2;
            }
        }

        var builder = new ChunkMeshBuilder(_blockTypes, blocks);
        var pool = builder.Build();
        var mesh = pool.Items;

        var indexBuffer = new IndexBuffer(_graphicsDevice,
            IndexElementSize.ThirtyTwoBits, mesh.TrianglesSize, BufferUsage.None);

        indexBuffer.SetData(mesh.InternalTriangles, 0, mesh.TrianglesSize);

        var vertexBuffer = new VertexBuffer(_graphicsDevice,
            typeof(VertexPositionTexture), mesh.TextureSize, BufferUsage.None);

        vertexBuffer.SetData(mesh.InternalTexture, 0, mesh.TextureSize);
        
        pool.RemoveMemoryUser();
        
        var response = new ChunkGenerateResponse(new ChunkInfo(pooledBlocks), 
            position, vertexBuffer, indexBuffer);

        _responses.Enqueue(response);
    }
}
