using RoyaResan.Mono2d.Animation;

namespace RoyaResan.Mono2d.Nodes
{
    public class SpriteNode : TransformNode
    {
        public Texture2D Texture;
        public Color Color = Color.White;

        public Animator Animator;

        public override void Update(GameTime gameTime)
        {
            Animator?.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(Renderer renderer)
        {
            Texture2D tex = Texture;

            if (Animator != null)
                tex = Animator.GetFrame();

            if (tex != null)
                renderer.DrawTexture(tex, GlobalPosition, Color);

            base.Draw(renderer);
        }
    }
}

