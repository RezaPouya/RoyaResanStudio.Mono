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

    public void Update(float dt)
    {
        CurrentScene?.Update(dt);
    }

    public void Draw(Rendering.Render renderer)
    {
        CurrentScene?.Draw(renderer);
    }
}