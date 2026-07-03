namespace RoyaResan.Mono2d.Core;

public class Scene
{
    public SceneNode Root = new SceneNode();

    public void Update(GameTime gameTime)
    {
        Root.Update(gameTime);
    }

    public void Draw(Renderer renderer)
    {
        Root.Draw(renderer);
    }
}
