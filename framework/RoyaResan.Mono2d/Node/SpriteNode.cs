using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoyaResan.Mono2d.Animation;


namespace RoyaResan.Mono2d.Node;

public class SpriteNode : TransformNode
{
    public Texture2D Texture;

    public Animator Animator = new();

    public override void Update(float dt)
    {
        base.Update(dt);
    }

    public void Update(GameTime gameTime)
    {
        Animator.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (Texture == null)
            return;

        var frame = Animator.CurrentFrame;

        spriteBatch.Draw(
            Texture,
            GlobalPosition,
            frame,
            Color.White
        );
    }
}