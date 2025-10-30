using Leopotam.EcsLite;

namespace CubeCity.Systems.Chunks.Models;

public class ChunkLoaderInfo(ChunkLoaderState state)
{
    public EcsPackedEntity? PackedEntity;
    public ChunkLoaderState State = state;
}
