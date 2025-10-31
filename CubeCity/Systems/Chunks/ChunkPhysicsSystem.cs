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
    private readonly EcsFilter _chunkUpdateFlagFilter = world.Filter<ChunkBlocksUpdateFlag>().End();
    private readonly EcsPool<ChunkPhysicsUnloadEvent> _chunkPhysicsUnloadEvents = world.GetPool<ChunkPhysicsUnloadEvent>();
    private readonly EcsFilter _chunkPhysicsUnloadFilter = world.Filter<ChunkPhysicsUnloadEvent>().End();

    public void Run(IEcsSystems systems)
    {
        foreach (var entity in _chunkUpdateFlagFilter)
        {
            ref var chunk = ref _chunkPool.Get(entity);
            ref var inWorldPos = ref _posPool.Get(entity);
            physicsEngine.AddChunkBlocks(chunk.Position, inWorldPos.Position, chunk.Blocks);
        }

        foreach (var entity in _chunkPhysicsUnloadFilter)
        {
            ref var chunk = ref _chunkPhysicsUnloadEvents.Get(entity);
            physicsEngine.RemoveChunkBlocks(chunk.ChunkPos);
            world.DelEntity(entity);
        }

        if (keyboard.IsKeyPressed(Keys.N))
        {
            var inter = physicsEngine.GetIntersection(camera.Position, camera.Forward);

            logger.LogInformation("Matched Count: {MatchedCount}; Total Count: {TotalCount}",
                inter.MatchedTriangles, inter.TotalTriangles);

            if (inter.Result is IntersectionResult result)
            {
                var triangle = result.Triangle;

                logger.LogInformation(
                    "Found intersection in ChunkPos: {ChunkPos} Distance: {Distance} Triangle: {P0}:{P1}:{P2}",
                    result.ChunkPos, result.Distance, triangle.P0, triangle.P1, triangle.P2);
            }
        }
    }
}
