namespace RoyaResan.Mono2d.Graphics;

public class Renderer
{
    private readonly SpriteBatch _spriteBatch;

    public Renderer(SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;
    }

    public void Begin()
    {
        _spriteBatch.Begin();
    }

    public void End()
    {
        _spriteBatch.End();
    }

    public void DrawTexture(Texture2D texture, Vector2 position, Color color)
    {
        _spriteBatch.Draw(texture, position, color);
    }
}
