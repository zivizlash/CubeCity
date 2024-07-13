using CubeCity.Physics.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CubeCity.Physics;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly PhysicsContainer _physicsContainer;

    private SpriteBatch _spriteBatch;
    private Texture2D _blocks;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _physicsContainer = new PhysicsContainer();
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _blocks = Content.Load<Texture2D>("box1");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed 
            || Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        // TODO: Add your update logic here

        _physicsContainer.Update(gameTime.ElapsedGameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

        _spriteBatch.Begin();

        foreach (var physicsBody in _physicsContainer.Bodies)
        {
            var aabb = physicsBody.Aabb;

            _spriteBatch.Draw(_blocks, aabb.GetPosition(), Color.White);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
