using RoyaResan.Mono2d.Physics;

namespace RoyaResan.Mono2d.Core;

public class Scene
{
    public SceneNode Root = new SceneNode();
    public Camera2D Camera = new Camera2D();
    public PhysicsWorld Physics = new PhysicsWorld();

    public void Update(GameTime gameTime)
    {
        Camera.Update();
        Root.Update(gameTime);
        Physics.Step();
    }

    public void Draw(Renderer renderer)
    {
        renderer.Camera = Camera;
        Root.Draw(renderer);
    }
}
