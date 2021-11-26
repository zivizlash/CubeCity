using System;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using CubeCity.GameObjects;
using CubeCity.Managers;
using CubeCity.Models;
using CubeCity.Tools;
using Microsoft.Xna.Framework.Graphics;

namespace CubeCity.Generators.Pipelines
{
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

    public class GeneratedChunkPoolManager
    {
        public ConcurrentDictionary<Vector2Int, Chunk> _chunks;

        public GeneratedChunkPoolManager()
        {
            _chunks = new();
        }
    }

    public class ChunkDataBinder
    {
        private readonly ConcurrentQueue<ChunkBindResponse> _responses;
        private readonly ActionBlock<ChunkBindRequest> _requests;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly BlockType[] _blockTypes;

        public ChunkDataBinder(GraphicsDevice graphicsDevice, BlockType[] blockTypes)
        {
            _responses = new ConcurrentQueue<ChunkBindResponse>();
            _requests = new ActionBlock<ChunkBindRequest>(
                Process, new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 1,
                    SingleProducerConstrained = true
                });
            _graphicsDevice = graphicsDevice;
            _blockTypes = blockTypes;
        }

        public void AddRequest(ChunkBindRequest request)
        {
            if (!_requests.Post(request))
                throw new InvalidOperationException();
        }

        public bool TryGetChunk(out ChunkBindResponse response)
        {
            return _responses.TryDequeue(out response);
        }

        private void Process(ChunkBindRequest request)
        {
            /*var builder = new ChunkMeshBuilder(_blockTypes, request.Blocks);
            var mesh = builder.Build();

            var indexBuffer = new IndexBuffer(_graphicsDevice, IndexElementSize.ThirtyTwoBits,
                mesh.Triangles.Length, BufferUsage.None);

            var verticesBuffer = new VertexBuffer(_graphicsDevice, 
                typeof(VertexPositionTexture), mesh.Vertices.Length, BufferUsage.None);

            indexBuffer.SetData(mesh.Triangles.Array, 0, mesh.Triangles.Length);
            verticesBuffer.SetData(mesh.Vertices.Array, 0, mesh.Triangles.Length);

            mesh.Triangles.Dispose();
            mesh.Vertices.Dispose();

            _responses.Enqueue(new ChunkBindResponse(indexBuffer, verticesBuffer, request.Blocks));*/
        }
    }
}
