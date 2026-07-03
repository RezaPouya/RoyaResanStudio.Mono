using RoyaResan.Mono2d.Physics;

namespace RoyaResan.Mono2d.Core;

public class Scene
{
    public SceneNode Root = new SceneNode();
    public Camera2D Camera = new Camera2D();
    public PhysicsWorld Physics = new PhysicsWorld();

    /// <summary>
    /// Adds a body to the node tree AND registers it with the physics
    /// world. Use this instead of Root.AddChild for any PhysicsBody,
    /// or it will never collide with anything.
    /// </summary>
    public void AddBody(PhysicsBody body, Node parent = null)
    {
        (parent ?? (Node)Root).AddChild(body);
        Physics.Bodies.Add(body);
    }

    public void Update(GameTime gameTime)
    {
        Root.Update(gameTime);
        Physics.Step();
        Camera.Update();
    }

    public void Draw(Renderer renderer)
    {
        renderer.Camera = Camera;
        Root.Draw(renderer);
    }
}
