namespace RoyaResan.Mono2d.Graphics;

public class Renderer
{
    private readonly SpriteBatch _spriteBatch;

    public Camera2D Camera;

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
        if (Camera != null)
            position = Camera.WorldToScreen(position);

        _spriteBatch.Draw(texture, position, color);
    }

    /// <summary>Draws a sub-rectangle of a texture - used for spritesheet frames and animation blending.</summary>
    public void DrawTexture(Texture2D texture, Rectangle? sourceRect, Vector2 position, Color color)
    {
        if (Camera != null)
            position = Camera.WorldToScreen(position);

        _spriteBatch.Draw(texture, position, sourceRect, color);
    }
}