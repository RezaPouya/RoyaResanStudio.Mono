namespace RoyaResan.Mono2d.UI;

/// <summary>
/// Rectangle + centered label + click handling. Hover/click both use
/// screen-space mouse position directly (Input.MousePosition), which is
/// correct here specifically because UI is drawn unaffected by Camera -
/// see Renderer's screen-space methods.
/// </summary>
public class UiButton : UiElement
{
    public SpriteFont Font = null!;
    public string Text = "";
    public Color NormalColor = new(60, 60, 60, 220);
    public Color HoverColor = new(90, 90, 90, 230);
    public Color TextColor = Color.White;
    public Action? OnClick;

    public bool IsHovered { get; private set; }

    public override void Update(GameTime gameTime, Vector2 parentScreenPosition)
    {
        base.Update(gameTime, parentScreenPosition);

        if (!Visible)
            return;

        var bounds = new Rectangle((int)ScreenPosition.X, (int)ScreenPosition.Y, (int)Size.X, (int)Size.Y);
        Vector2 mouse = InputManager.MousePosition;
        IsHovered = bounds.Contains((int)mouse.X, (int)mouse.Y);

        if (IsHovered && InputManager.LeftPressed)
            OnClick?.Invoke();
    }

    public override void Draw(Renderer renderer)
    {
        if (!Visible)
            return;

        var bounds = new Rectangle((int)ScreenPosition.X, (int)ScreenPosition.Y, (int)Size.X, (int)Size.Y);
        renderer.DrawRect(bounds, IsHovered ? HoverColor : NormalColor);

        if (Font != null && !string.IsNullOrEmpty(Text))
        {
            Vector2 textSize = Font.MeasureString(Text);
            Vector2 textPos = ScreenPosition + (Size - textSize) / 2f;
            renderer.DrawText(Font, Text, textPos, TextColor);
        }

        base.Draw(renderer);
    }
}