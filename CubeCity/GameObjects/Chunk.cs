using CubeCity.Models;
using CubeCity.Tools;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace CubeCity.GameObjects
{
    public static class PerformanceCounter
    {
        private static readonly ConcurrentDictionary<int, int> _dict;

        static PerformanceCounter()
        {
            _dict = new ConcurrentDictionary<int, int>();
        }

        public static void AddChunkInfo(int indexBufferCount)
        {
            return;
            var granulated = indexBufferCount - indexBufferCount % 1024;
            _dict.AddOrUpdate(granulated, 1, (_, value) => value + 1);
        }

        public static void SaveReport()
        {
            return;
            Directory.CreateDirectory("Reports");
            
            var sorted = _dict
                .OrderByDescending(key => key.Value)
                .Select(key => new { count = key.Value, size = key.Key });

            File.WriteAllText($"Reports\\Report {DateTime.Now:yyyy-MM-dd-hh-mm-ss}.json", 
                JsonConvert.SerializeObject(sorted));
        }
    }

    public class ChunkDrawData : IDisposable
    {
        public ushort[,,] Blocks
        {
            get
            {
                CheckDisposed();
                return _blocks.Resource;
            }
        }

        private Pooled<ushort[,,]> _blocks;
        private bool _disposed;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly IndexBuffer _indexBuffer;
        private readonly VertexBuffer _vertexBuffer;

        public ChunkDrawData(GraphicsDevice graphicsDevice, Pooled<ushort[,,]> blocks,
            IndexBuffer indexBuffer, VertexBuffer vertexBuffer)
        {
            _indexBuffer = indexBuffer;
            _vertexBuffer = vertexBuffer;
            _graphicsDevice = graphicsDevice;
            _blocks = blocks;

            PerformanceCounter.AddChunkInfo(_vertexBuffer.VertexCount);
        }
        
        public void Draw()
        {
            CheckDisposed();

            _graphicsDevice.Indices = _indexBuffer;
            _graphicsDevice.SetVertexBuffer(_vertexBuffer);

            _graphicsDevice.DrawIndexedPrimitives(
                PrimitiveType.TriangleList, 0, 0, _indexBuffer.IndexCount / 3);
        }

        /*
        public void Update(SimpleMesh mesh, Pooled<ushort[,,]> blocks)
        {
            CheckDisposed();
            DisposePooledObjects();

            _blocks = blocks;

            _indexBuffer.SetData(mesh.Triangles.Array, 0, mesh.Triangles.Length);
            _vertexBuffer.SetData(mesh.Vertices.Array, 0, mesh.Vertices.Length);
        }
        */

        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Chunk));
        }

        private void DisposePooledObjects()
        {
            //Mesh.Triangles.Dispose();
            //Mesh.Vertices.Dispose();
            _blocks.Dispose();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            DisposePooledObjects();

            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
        }
    }

    public class Chunk
    {
        public Vector2Int Position { get; set; }
        public ChunkDrawData? DrawData { get; private set; }
        public bool IsInWorld => DrawData != null;
        public ushort[,,]? Blocks => DrawData?.Blocks;

        private readonly GraphicsDevice _graphicsDevice;

        public Chunk(GraphicsDevice graphicsDevice, Vector2Int position)
        {
            _graphicsDevice = graphicsDevice;
            Position = position;
        }

        public void Update(Pooled<ushort[,,]> blocks, IndexBuffer indexBuffer, VertexBuffer vertexBuffer)
        {
            if (DrawData == null)
            {
                DrawData = new ChunkDrawData(_graphicsDevice, blocks, indexBuffer, vertexBuffer);
            }
            else
            {
                throw new InvalidOperationException();
                //DrawData.Update(mesh, blocks);
            }
        }

        public void Release()
        {
            DrawData?.Dispose();
        }

        public void Draw()
        {
            DrawData?.Draw();
        }
    }
}
