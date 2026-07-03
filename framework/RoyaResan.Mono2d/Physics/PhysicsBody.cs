namespace RoyaResan.Mono2d.Physics;

public class PhysicsBody : TransformNode
{
    public Vector2 Velocity;

    public Collider Collider;

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Position += Velocity * dt;

        base.Update(gameTime);
    }
}