namespace RoyaResan.Mono2d.Nodes;

/// <summary>
/// A colored rectangle drawn centered on GlobalPosition - the
/// placeholder-art stand-in for a SpriteNode until real spritesheets
/// exist. Size/Color are public so a driver script (see
/// Gameplay/PlaceholderPoseScript) can change them per animation state
/// - e.g. shrink+lean for Crouch/Run, tint red on Hurt.
///
/// Deliberately centered on GlobalPosition (unlike SpriteNode, which is
/// top-left-origin) so the rect visually matches this body's Collider,
/// which is also center-origin. If you later swap this for a real
/// SpriteNode with actual art, remember that origin convention changes.
/// </summary>
public class PlaceholderRectNode : TransformNode
{
    public Vector2 Size = new Vector2(24, 48);
    public Color Color = Color.White;
    public bool Visible = true;

    public override void Draw(Renderer renderer)
    {
        if (Visible)
            renderer.DrawRectWorld(GlobalPosition, Size, Color);

        base.Draw(renderer);
    }
}