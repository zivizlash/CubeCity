using CubeCity.Components;
using CubeCity.GameObjects;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeCity.Systems.Render;

public class RenderSystem(EcsWorld world, GraphicsDevice graphicsDevice, RasterizerState rasterizerState,
    Camera camera, BasicEffect effect, Texture2D texture) : IEcsRunSystem
{
    private readonly Matrix _worldMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
    private readonly EcsFilter _drawingFilter = world.Filter<RenderComponent>().Inc<PositionComponent>().End();
    private readonly EcsPool<RenderComponent> _drawingPool = world.GetPool<RenderComponent>();
    private readonly EcsPool<PositionComponent> _positionPool = world.GetPool<PositionComponent>();

    public void Run(IEcsSystems systems)
    {
        graphicsDevice.BlendState = BlendState.Opaque;
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = rasterizerState;

        effect.View = camera.ViewMatrix;
        effect.Projection = camera.ProjectionMatrix;
        effect.Texture = texture;

        foreach (var entity in _drawingFilter)
        {
            ref var drawing = ref _drawingPool.Get(entity);
            ref var position = ref _positionPool.Get(entity);

            var translation = Matrix.CreateTranslation(position.Position);
            effect.World = _worldMatrix * translation;

            graphicsDevice.Indices = drawing.IndexBuffer;
            graphicsDevice.SetVertexBuffer(drawing.VertexBuffer);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 
                    drawing.IndexBuffer.IndexCount / 3);
            }
        }
    }
}
