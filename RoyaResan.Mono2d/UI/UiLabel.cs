namespace RoyaResan.Mono2d.UI;

/// <summary>Static or dynamic text - set Text every frame from game code for things like a HUD score/health readout.</summary>
public class UiLabel : UiElement
{
    public SpriteFont Font = null!;
    public string Text = "";
    public Color Color = Color.White;

    public override void Draw(Renderer renderer)
    {
        if (!Visible)
            return;

        renderer.DrawText(Font, Text, ScreenPosition, Color);
        base.Draw(renderer);
    }
}