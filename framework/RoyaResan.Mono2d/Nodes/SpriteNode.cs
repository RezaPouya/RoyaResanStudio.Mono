using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RoyaResan.Mono2d.Animation;

namespace RoyaResan.Mono2d.Nodes;

public class SpriteNode : TransformNode
{
    public Texture2D Texture;
    public Animator Animator = new();

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
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