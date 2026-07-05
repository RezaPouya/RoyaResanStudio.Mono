namespace RoyaResan.Mono2d.UI;

/// <summary>
/// Simple screen stack. Push a built UiElement tree (a pause menu, a HUD,
/// a dialog) to show it; Pop to remove it. Only the top of the stack gets
/// Update/Draw - so a pause menu on top doesn't also feed clicks through
/// to a HUD sitting underneath it.
///
/// If you want a HUD visible AT THE SAME TIME as gameplay (health bar
/// while playing, not just in menus), don't push it through here - just
/// keep a UiElement reference in game code and call its Update/Draw
/// directly alongside the stack, or push it first and never pop it while
/// treating "pause menu on top" as a second, separate push.
/// </summary>
public class UiManager
{
    private readonly Stack<UiElement> _stack = new();

    public bool HasScreen => _stack.Count > 0;
    public UiElement? Current => _stack.Count > 0 ? _stack.Peek() : null;

    public void Push(UiElement screen) => _stack.Push(screen);

    public UiElement? Pop() => _stack.Count > 0 ? _stack.Pop() : null;

    public void Update(GameTime gameTime)
    {
        if (_stack.Count > 0)
            _stack.Peek().Update(gameTime, Vector2.Zero);
    }

    public void Draw(Renderer renderer)
    {
        if (_stack.Count > 0)
            _stack.Peek().Draw(renderer);
    }
}