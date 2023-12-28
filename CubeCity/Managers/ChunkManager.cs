using System;
using System.Collections.Generic;
using CubeCity.GameObjects;
using CubeCity.Generators;
using CubeCity.Generators.Pipelines;
using CubeCity.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeCity.Managers
{
    public class ChunkManager
    {
        private readonly Dictionary<Vector2Int, Chunk> _chunks;
        private readonly ChunkGenerator _chunkGenerator;
        private readonly GraphicsDevice _graphicsDevice;

        public ChunkManager(ChunkGenerator chunkGenerator, GraphicsDevice graphicsDevice)
        {
            _chunkGenerator = chunkGenerator;
            _graphicsDevice = graphicsDevice;
            _chunks = new Dictionary<Vector2Int, Chunk>(256);
        }

        public int ChunksCount => _chunks.Count;

        public bool RemoveChunk(Vector2Int position)
        {
            if (_chunks.TryGetValue(position, out var chunk) && chunk.IsInWorld)
            {
                chunk.Release();
                _chunks.Remove(position);
                return true;
            }

            return false;
        }

        public Chunk GenerateChunk(Vector2Int position)
        {
            if (_chunks.TryGetValue(position, out var cached))
                return cached;

            var chunk = new Chunk(_graphicsDevice, position);
            _chunks.Add(position, chunk);
            _chunkGenerator.AddGenerationRequest(new ChunkGenerateRequest(position));

            return chunk;
        }

        public bool TryGetGeneratedChunks(out ChunkGenerateResponse response)
        {
            return _chunkGenerator.TryGetChunk(out response);
        }

        public void Draw(Matrix worldMatrix, BasicEffect effect)
        {
            foreach (var chunk in _chunks.Values)
            {
                var translation = Matrix.CreateTranslation(
                    new Vector3(chunk.Position.X * 16, 0, chunk.Position.Y * 16));

                effect.World = worldMatrix * translation;

                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    chunk.Draw();
                }
            }
        }

        public void Update(TimeSpan elapsed)
        {
            while (_chunkGenerator.TryGetChunk(out var generated))
            {
                if (_chunks.TryGetValue(generated.Position, out var chunk))
                {
                    chunk.Update(generated.ChunkInfo.Blocks, generated.IndexBuffer, generated.VertexBuffer);
                }
            }
        }
    }
}
