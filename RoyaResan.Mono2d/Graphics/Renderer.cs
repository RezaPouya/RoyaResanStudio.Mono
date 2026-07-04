namespace RoyaResan.Mono2d.Graphics;

public class Renderer
{
    private readonly SpriteBatch _spriteBatch;
    private Texture2D? _pixel;

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

    // --------------------
    // Screen-space (UI/HUD) - deliberately ignore Camera. World content
    // uses the methods above and scrolls with the camera; UI is drawn in
    // fixed screen coordinates on top of it.
    // --------------------

    /// <summary>1x1 white pixel, created lazily - lets DrawRect work without shipping a texture asset.</summary>
    private Texture2D Pixel
    {
        get
        {
            if (_pixel == null)
            {
                _pixel = new Texture2D(_spriteBatch.GraphicsDevice, 1, 1);
                _pixel.SetData(new[] { Color.White });
            }

            return _pixel;
        }
    }

    /// <summary>Draws a texture at a fixed screen position, ignoring Camera - for UI icons/backgrounds.</summary>
    public void DrawTextureScreen(Texture2D texture, Rectangle? sourceRect, Vector2 screenPosition, Color color)
    {
        _spriteBatch.Draw(texture, screenPosition, sourceRect, color);
    }

    /// <summary>Solid-color filled rectangle in screen space - panel/button backgrounds, health bars, etc.</summary>
    public void DrawRect(Rectangle screenRect, Color color)
    {
        _spriteBatch.Draw(Pixel, screenRect, color);
    }

    /// <summary>Screen-space text. Caller loads/owns the SpriteFont (Content.Load&lt;SpriteFont&gt;).</summary>
    public void DrawText(SpriteFont font, string text, Vector2 screenPosition, Color color)
    {
        _spriteBatch.DrawString(font, text, screenPosition, color);
    }
}