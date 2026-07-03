namespace RoyaResan.Mono2d.Nodes;

public class PlayerNode : TransformNode
{
    public float Speed = 200f;

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Vector2 move = Vector2.Zero;

        if (Input.IsKeyDown(Keys.W))
            move.Y -= 1;

        if (Input.IsKeyDown(Keys.S))
            move.Y += 1;

        if (Input.IsKeyDown(Keys.A))
            move.X -= 1;

        if (Input.IsKeyDown(Keys.D))
            move.X += 1;

        if (move != Vector2.Zero)
            move.Normalize();

        Position += move * Speed * dt;

        base.Update(gameTime);
    }
}