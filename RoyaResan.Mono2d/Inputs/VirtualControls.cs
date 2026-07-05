using Microsoft.Xna.Framework.Input.Touch;

namespace RoyaResan.Mono2d.Inputs;

/// <summary>
/// Virtual on-screen controls with dynamic scaling + platform hiding.
/// </summary>
public static class VirtualControls
{
    private static Rectangle _leftRect, _rightRect, _jumpRect, _attackRect, _throwRect;

    private static readonly HashSet<string> _activeTouches = new();

    public static bool ShowVirtualControls { get; set; } = true; // Set false on Desktop

    public static void Initialize(GraphicsDevice graphics)
    {
        UpdateLayout(graphics.Viewport.Width, graphics.Viewport.Height);
    }

    public static void UpdateLayout(int screenWidth, int screenHeight)
    {
        int buttonSize = screenWidth / 10; // Dynamic ~10% of width
        int padding = buttonSize / 2;

        // Bottom-left movement
        _leftRect = new Rectangle(padding, screenHeight - buttonSize - padding, buttonSize, buttonSize);
        _rightRect = new Rectangle(padding * 3 + buttonSize, screenHeight - buttonSize - padding, buttonSize, buttonSize);

        // Right side actions
        _jumpRect = new Rectangle(screenWidth - buttonSize * 2 - padding, screenHeight - buttonSize * 2 - padding * 2, buttonSize, buttonSize);
        _attackRect = new Rectangle(screenWidth - buttonSize - padding, screenHeight - buttonSize - padding, buttonSize, buttonSize);
        _throwRect = new Rectangle(screenWidth - buttonSize * 2 - padding, screenHeight - buttonSize - padding, buttonSize, buttonSize);
    }

    public static void Update()
    {
        if (!ShowVirtualControls) return;

        _activeTouches.Clear();

        foreach (var touch in TouchPanel.GetState())
        {
            if (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved)
            {
                var pos = touch.Position.ToPoint();

                if (_leftRect.Contains(pos)) _activeTouches.Add("Left");
                if (_rightRect.Contains(pos)) _activeTouches.Add("Right");
                if (_jumpRect.Contains(pos)) _activeTouches.Add("Jump");
                if (_attackRect.Contains(pos)) _activeTouches.Add("Attack");
                if (_throwRect.Contains(pos)) _activeTouches.Add("Throw");
            }
        }
    }

    public static bool IsVirtualDown(string actionName) => _activeTouches.Contains(actionName);

    public static void Draw(Renderer renderer)
    {
        if (!ShowVirtualControls) return;

        Color tint = Color.White * 0.4f;
        renderer.DrawRect(_leftRect, tint);
        renderer.DrawRect(_rightRect, tint);
        renderer.DrawRect(_jumpRect, tint);
        renderer.DrawRect(_attackRect, tint);
        renderer.DrawRect(_throwRect, tint);
    }
}