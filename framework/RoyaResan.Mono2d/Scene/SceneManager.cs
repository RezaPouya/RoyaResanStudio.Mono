using Microsoft.Xna.Framework;

namespace RoyaResan.Mono2d.Scene;

public class SceneManager
{
    public Scene CurrentScene { get; private set; }

    public void ChangeScene(Scene scene)
    {
        CurrentScene?.Unload();
        CurrentScene = scene;
        CurrentScene.Load();
    }

    public void Update(GameTime gameTime)
    {
        CurrentScene?.Update(gameTime);
    }

    public void Draw(Rendering.Render renderer)
    {
        CurrentScene?.Draw(renderer);
    }
}