using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoyaResan.Mono2d.Core;
using RoyaResan.Mono2d.Graphics;
using RoyaResan.Mono2d.Nodes;

namespace RoyaResan.ShadowDancer.Desktop;

public class World : Game
{
    private GraphicsDeviceManager _graphics;
    private Renderer _renderer;

    private Scene _scene;
    private Texture2D _testTexture;

    public World()
    {
        _graphics = new GraphicsDeviceManager(this);
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
        var spriteBatch = new SpriteBatch(GraphicsDevice);
        _renderer = new Renderer(spriteBatch);

        // Load a test texture (put a PNG in Content folder)
        _testTexture = Content.Load<Texture2D>("test");

        // Create scene
        _scene = new Scene();

        // Create sprite node
        var sprite = new SpriteNode
        {
            Texture = _testTexture,
            Position = new Vector2(100, 100)
        };

        _scene.Root.AddChild(sprite);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        _scene.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _renderer.Begin();

        _scene.Draw(_renderer);

        _renderer.End();

        base.Draw(gameTime);
    }
}
