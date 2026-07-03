namespace RoyaResan.Mono2d.Inputs;

public static class Input
{
    private static KeyboardState _currentKeyboard;
    private static KeyboardState _previousKeyboard;

    private static MouseState _currentMouse;
    private static MouseState _previousMouse;

    public static void Update()
    {
        _previousKeyboard = _currentKeyboard;
        _currentKeyboard = Keyboard.GetState();

        _previousMouse = _currentMouse;
        _currentMouse = Mouse.GetState();
    }

    // --------------------
    // Keyboard
    // --------------------

    public static bool IsKeyDown(Keys key)
        => _currentKeyboard.IsKeyDown(key);

    public static bool IsKeyPressed(Keys key)
        => _currentKeyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);

    public static bool IsKeyReleased(Keys key)
        => !_currentKeyboard.IsKeyDown(key) && _previousKeyboard.IsKeyDown(key);

    // --------------------
    // Mouse
    // --------------------

    public static Vector2 MousePosition
        => new Vector2(_currentMouse.X, _currentMouse.Y);

    public static bool LeftClick
        => _currentMouse.LeftButton == ButtonState.Pressed;

    public static bool LeftPressed
        => _currentMouse.LeftButton == ButtonState.Pressed &&
           _previousMouse.LeftButton == ButtonState.Released;

    public static bool LeftReleased
        => _currentMouse.LeftButton == ButtonState.Released &&
           _previousMouse.LeftButton == ButtonState.Pressed;

    public static bool RightClick
        => _currentMouse.RightButton == ButtonState.Pressed;

    public static int ScrollWheelDelta
        => _currentMouse.ScrollWheelValue - _previousMouse.ScrollWheelValue;
}