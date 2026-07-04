using RoyaResan.Mono2d.Graphics;

namespace RoyaResan.Mono2d.UI;

/// <summary>
/// Minimal retained-mode UI node. Deliberately a separate, much simpler
/// tree than Nodes/TransformNode - UI doesn't need physics, colliders, or
/// world-space parenting, just screen-space rectangles. Position is
/// relative to the parent (root elements are pushed with an absolute
/// screen position by UiManager).
/// </summary>
public class UiElement
{
    public Vector2 Position;
    public Vector2 Size;
    public bool Visible = true;
    public List<UiElement> Children = new();

    /// <summary>Position plus every ancestor's Position - computed each Update, read during Draw.</summary>
    public Vector2 ScreenPosition { get; private set; }

    public void AddChild(UiElement child) => Children.Add(child);

    public virtual void Update(GameTime gameTime, Vector2 parentScreenPosition)
    {
        if (!Visible)
            return;

        ScreenPosition = parentScreenPosition + Position;

        foreach (var child in Children)
            child.Update(gameTime, ScreenPosition);
    }

    public virtual void Draw(Renderer renderer)
    {
        if (!Visible)
            return;

        foreach (var child in Children)
            child.Draw(renderer);
    }
}
