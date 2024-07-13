using CubeCity.Components;
using CubeCity.GameObjects;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CubeCity.Systems;

public class RenderSystem(GraphicsDevice graphicsDevice, RasterizerState rasterizerState,
    Camera camera, BasicEffect effect, Texture2D texture) : IEcsInitSystem, IEcsRunSystem
{
    private readonly GraphicsDevice _graphicsDevice = graphicsDevice;
    private readonly RasterizerState _rasterizerState = rasterizerState;
    private readonly Camera _camera = camera;
    private readonly BasicEffect _effect = effect;
    private readonly Texture2D _texture = texture;
    private readonly Matrix _worldMatrix = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);

    private EcsFilter _drawingFilter = null!;
    private EcsPool<RenderComponent> _drawingPool = null!;
    private EcsPool<PositionComponent> _positionPool = null!;

    public void Init(IEcsSystems systems)
    {
        var world = systems.GetWorld();
        _positionPool = world.GetPool<PositionComponent>();
        _drawingPool = world.GetPool<RenderComponent>();
        _drawingFilter = world.Filter<RenderComponent>().Inc<PositionComponent>().End();
    }

    public void Run(IEcsSystems systems)
    {
        _graphicsDevice.BlendState = BlendState.Opaque;
        _graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        _graphicsDevice.RasterizerState = _rasterizerState;

        _effect.View = _camera.ViewMatrix;
        _effect.Projection = _camera.ProjectionMatrix;
        _effect.Texture = _texture;

        foreach (var entity in _drawingFilter)
        {
            ref var drawing = ref _drawingPool.Get(entity);
            ref var position = ref _positionPool.Get(entity);

            var translation = Matrix.CreateTranslation(position.Position);
            _effect.World = _worldMatrix * translation;

            _graphicsDevice.Indices = drawing.IndexBuffer;
            _graphicsDevice.SetVertexBuffer(drawing.VertexBuffer);

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                _graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 
                    drawing.IndexBuffer.IndexCount / 3);
            }
        }
    }
}
