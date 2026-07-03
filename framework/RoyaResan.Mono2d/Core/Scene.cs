namespace RoyaResan.Mono2d.Core;

public class Scene
{
    public SceneNode Root = new SceneNode();
    public Camera2D Camera = new Camera2D();

    public void Update(GameTime gameTime)
    {
        Camera.Update();
        Root.Update(gameTime);
    }

    public void Draw(Renderer renderer)
    {
        renderer.Camera = Camera;
        Root.Draw(renderer);
    }
}
