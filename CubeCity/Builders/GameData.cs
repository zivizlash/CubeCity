using CubeCity.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CubeCity.Builders;

public class GameData
{
    public GameSettings Settings { get; }
    public Texture2D BlocksTexture { get; }
    public SpriteBatch SpriteBatch { get; }
    public SpriteFont SpriteFont { get; }
    public GameWindow Window { get; }
    public GraphicsDeviceManager GraphicsManager { get; }
    public GraphicsDevice GraphicsDevice { get; }
    public ITime Time { get; }
    public Action Exit { get; }

    public GameData(Texture2D blocksTexture, SpriteBatch spriteBatch, SpriteFont spriteFont,
        GraphicsDeviceManager graphicsManager, ITime time, GameSettings settings,
        GameWindow window, GraphicsDevice graphicsDevice, Action exit)
    {
        BlocksTexture = blocksTexture;
        SpriteBatch = spriteBatch;
        SpriteFont = spriteFont;
        GraphicsManager = graphicsManager;
        Time = time;
        Settings = settings;
        Window = window;
        GraphicsDevice = graphicsDevice;
        Exit = exit;
    }
}
