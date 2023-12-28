using CubeCity.Models;
using CubeCity.Tools;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CubeCity.Generators.Pipelines;

public class GuarantorNearChunksGenerated
{
    private readonly Dictionary<Vector2Int, NearChunkInfoInternal> _waitingChunks;
    private readonly Queue<NearChunkInfoInternal> _queue;

    private static readonly Vector2Int Left   = new(-1,  0);
    private static readonly Vector2Int Right  = new( 1,  0);
    private static readonly Vector2Int Up     = new( 0,  1);
    private static readonly Vector2Int Bottom = new( 0, -1);
    
    public GuarantorNearChunksGenerated()
    {
        _waitingChunks = new();
        _queue = new();
    }
    
    public void AddChunk(Vector2Int position, ChunkInfo chunkInfo)
    {
        // ReSharper disable once InlineOutVariableDeclaration
        NearChunkInfoInternal info;

        if (_waitingChunks.TryGetValue(position + Up, out info))
        {
            info.Bottom = chunkInfo;
            Update(position + Up, info);
        }

        if (_waitingChunks.TryGetValue(position + Bottom, out info))
        {
            info.Up = chunkInfo;
            Update(position + Bottom, info);
        }

        if (_waitingChunks.TryGetValue(position + Left, out info))
        {
            info.Right = chunkInfo;
            Update(position + Left, info);
        }

        if (_waitingChunks.TryGetValue(position + Right, out info))
        {
            info.Left = chunkInfo;
            Update(position + Right, info);
        }

        if (_waitingChunks.TryGetValue(position, out info))
        {
            info.Center = chunkInfo;
            Update(position, info);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Update(Vector2Int position, NearChunkInfoInternal info)
    {
        if (info.IsFilled)
        {
            _waitingChunks.Remove(position);
            _queue.Enqueue(info);
        }
        else
        {
            _waitingChunks[position] = info;
        }
    }

    public bool TryGetChunkWithNear(out NearChunksInfo chunksInfo)
    {
        if (_queue.TryDequeue(out var info))
        {
            chunksInfo = new NearChunksInfo();
        }

        chunksInfo = default;
        return false;
    }

    private struct NearChunkInfoInternal
    {
        public ChunkInfo? Left { get; set; }
        public ChunkInfo? Right { get; set; }
        public ChunkInfo? Up { get; set; }
        public ChunkInfo? Bottom { get; set; }
        public ChunkInfo? Center { get; set; }

        public bool IsFilled =>
            Left.HasValue && Right.HasValue &&
            Up.HasValue && Bottom.HasValue &&
            Center.HasValue;
    }
}
