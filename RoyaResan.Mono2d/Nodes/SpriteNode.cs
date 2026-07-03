using RoyaResan.Mono2d.Animation;

namespace RoyaResan.Mono2d.Nodes
{
    public class SpriteNode : TransformNode
    {
        public Texture2D Texture;
        public Rectangle? SourceRect; // only used when there's no Animator
        public Color Color = Color.White;

        public Animator Animator;

        public override void Update(GameTime gameTime)
        {
            Animator?.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(Renderer renderer)
        {
            if (Animator != null)
            {
                // Fade-out layer (outgoing state) drawn first, fade-in
                // layer (incoming state) drawn on top - a simple 2-frame
                // alpha crossfade, no extra art required.
                if (Animator.IsBlending)
                {
                    var prev = Animator.PreviousFrame;
                    if (prev.texture != null)
                        renderer.DrawTexture(prev.texture, prev.rect, GlobalPosition, Color * (1f - Animator.BlendWeight));
                }

                var cur = Animator.CurrentFrame;
                if (cur.texture != null)
                    renderer.DrawTexture(cur.texture, cur.rect, GlobalPosition, Color * Animator.BlendWeight);
            }
            else if (Texture != null)
            {
                renderer.DrawTexture(Texture, SourceRect, GlobalPosition, Color);
            }

            base.Draw(renderer);
        }
    }
}

