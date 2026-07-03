using RoyaResan.Mono2d.Graphics;

namespace RoyaResan.Mono2d.Nodes
{
    public class SpriteNode : Node
    {
        public Texture2D Texture;
        public Vector2 Position;
        public Color Color = Color.White;

        public override void Draw(Renderer renderer)
        {
            if (Texture != null)
            {
                renderer.DrawTexture(Texture, Position, Color);
            }

            base.Draw(renderer);
        }
    }
}