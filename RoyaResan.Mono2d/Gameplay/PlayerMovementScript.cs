using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay
{
    public class PlayerMovementScript : Script
    {
        public float Speed = 200f;

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 move = Vector2.Zero;

            if (InputManager.IsActionDown(InputManager.Up)) move.Y -= 1;
            if (InputManager.IsActionDown(InputManager.Down)) move.Y += 1;
            if (InputManager.IsActionDown(InputManager.Left)) move.X -= 1;
            if (InputManager.IsActionDown(InputManager.Right)) move.X += 1;

            if (move != Vector2.Zero)
                move.Normalize();

            Owner.Velocity = move * Speed;
        }
    }
}