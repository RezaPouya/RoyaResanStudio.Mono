using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoyaResan.Mono2d.Node;

namespace RoyaResan.Mono2d.Rendering;

public class Render
{
    private readonly SpriteBatch _spriteBatch;

    public Camera2D Camera { get; set; }

    public Render(SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;
    }

    public void Begin()
    {
        _spriteBatch.Begin(
            transformMatrix: Camera?.GetViewMatrix() ?? Matrix.Identity,
            samplerState: SamplerState.PointClamp
        );
    }

    public void End()
    {
        _spriteBatch.End();
    }

    // -----------------------------
    // DRAW SPRITE NODE
    // -----------------------------
    public void DrawSprite(SpriteNode node)
    {
        if (node.Texture == null)
            return;

        var frame = node.Animator.CurrentFrame;

        _spriteBatch.Draw(
            node.Texture,
            node.GlobalPosition,
            frame,
            Color.White
        );
    }

    // -----------------------------
    // DRAW GENERIC TEXTURE
    // -----------------------------
    public void Draw(Texture2D texture, Vector2 position, Rectangle? source = null)
    {
        _spriteBatch.Draw(texture, position, source, Color.White);
    }
}