using CubeCity.Components;
using CubeCity.GameObjects;
using CubeCity.Input;
using CubeCity.Physics;
using Leopotam.EcsLite;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Input;

namespace CubeCity.Systems.Chunks;

public class ChunkPhysicsSystem(EcsWorld world, PhysicsEngine physicsEngine, 
    KeyboardInputManager keyboard, Camera camera, ILogger<ChunkPhysicsSystem> logger) : IEcsRunSystem
{
    private readonly EcsPool<ChunkComponent> _chunkPool = world.GetPool<ChunkComponent>();
    private readonly EcsPool<PositionComponent> _posPool = world.GetPool<PositionComponent>();

    private readonly EcsFilter _chunkUpdateFlagsFilter = world.Filter<ChunkUpdateFlag>().End();
    private readonly EcsFilter _chunkUnloadFlagsFilter = world.Filter<ChunkUnloadFlag>().End();

    public void Run(IEcsSystems systems)
    {
        foreach (var entity in _chunkUpdateFlagsFilter)
        {
            ref var chunk = ref _chunkPool.Get(entity);
            ref var inWorldPos = ref _posPool.Get(entity);
            physicsEngine.AddOrUpdateChunkBlocks(chunk.Position, inWorldPos.Position, chunk.Blocks);
        }

        foreach (var entity in _chunkUnloadFlagsFilter)
        {
            ref var chunk = ref _chunkPool.Get(entity);
            physicsEngine.RemoveChunkBlocks(chunk.Position);
        }

        if (keyboard.IsKeyPressed(Keys.N))
        {
            var raycast = physicsEngine.Raycast(camera.Position, camera.Forward);

            logger.LogInformation("Matched Count: {MatchedCount}; Total Count: {TotalCount}",
                raycast.MatchedCount, raycast.TotalCount);

            if (raycast.Result is IntersectionResult result)
            {
                var triangle = result.Triangle;

                logger.LogInformation(
                    "Found intersection in ChunkPos: {ChunkPos} Distance: {Distance} Triangle: {P0}:{P1}:{P2}",
                    result.ChunkPos, result.Distance, triangle.P0, triangle.P1, triangle.P2);
            }
        }
    }
}
