namespace RoyaResan.Mono2d.Nodes
{
    public class SpriteNode : TransformNode
    {
        public Texture2D Texture;
        public Color Color = Color.White;

        public override void Draw(Renderer renderer)
        {
            if (Texture != null)
            {
                renderer.DrawTexture(Texture, GlobalPosition, Color);
            }

            base.Draw(renderer);
        }
    }
}