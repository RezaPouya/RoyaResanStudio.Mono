// RoyaResan.Mono2d/Inputs/InputAction.cs
namespace RoyaResan.Mono2d.Inputs;

public static class InputManager
{
    private static KeyboardState _currentKeyboard;
    private static KeyboardState _previousKeyboard;
    private static MouseState _currentMouse;
    private static MouseState _previousMouse;
    private static GamePadState _previousGamePadState = new GamePadState();

    private static readonly Dictionary<string, InputAction> _actions = new();

    public static InputAction Jump { get; private set; }
    public static InputAction Left { get; private set; }
    public static InputAction Right { get; private set; }
    public static InputAction Up { get; private set; }
    public static InputAction Down { get; private set; }
    public static InputAction Crouch { get; private set; }
    public static InputAction Attack { get; private set; }
    public static InputAction Throw { get; private set; }
    public static InputAction Rope { get; private set; }
    public static InputAction Menu { get; private set; }

    public static void Initialize()
    {
        Jump = Create("Jump", Keys.Space, Buttons.A);
        Left = Create("Left", Keys.Left, Buttons.DPadLeft, Buttons.LeftThumbstickLeft);
        Right = Create("Right", Keys.Right, Buttons.DPadRight, Buttons.LeftThumbstickRight);
        Up = Create("Up", Keys.Up, Buttons.DPadUp, Buttons.LeftThumbstickUp);
        Down = Create("Down", Keys.Down, Buttons.DPadDown, Buttons.LeftThumbstickDown);
        Crouch = Create("Crouch", Keys.Z, Buttons.DPadDown);
        Attack = Create("Attack", Keys.S, Buttons.X);
        Throw = Create("Throw", Keys.Q, Buttons.Y);  // Note: S for both Crouch and Throw - change if needed
        Rope = Create("Rope", Keys.R, Buttons.RightTrigger);
        Menu = Create("Menu", Keys.Escape, Buttons.Start);
    }

    private static InputAction Create(string name, Keys defaultKey, params Buttons[] defaultButtons)
    {
        var action = new InputAction(name);
        action.AddKeyboardKey(defaultKey);
        foreach (var b in defaultButtons) action.AddGamepadButton(b);
        _actions[name] = action;
        return action;
    }

    public static void Update()
    {
        _previousKeyboard = _currentKeyboard;
        _currentKeyboard = Keyboard.GetState();

        _previousMouse = _currentMouse;
        _currentMouse = Mouse.GetState();

        _previousGamePadState = GamePad.GetState(PlayerIndex.One);
    }

    public static bool IsActionDown(InputAction action)
    {
        foreach (var key in action.KeyboardKeys)
            if (_currentKeyboard.IsKeyDown(key)) return true;

        GamePadState pad = GamePad.GetState(PlayerIndex.One);
        if (pad.IsConnected)
        {
            foreach (var b in action.GamepadButtons)
                if (pad.IsButtonDown(b)) return true;

            if (action == Left && pad.ThumbSticks.Left.X < -0.5f) return true;
            if (action == Right && pad.ThumbSticks.Left.X > 0.5f) return true;
        }

        if (VirtualControls.IsVirtualDown(action.Name)) return true;

        return false;
    }

    public static bool IsActionPressed(InputAction action)
    {
        foreach (var key in action.KeyboardKeys)
            if (_currentKeyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key)) return true;

        GamePadState pad = GamePad.GetState(PlayerIndex.One);
        if (pad.IsConnected)
        {
            foreach (var b in action.GamepadButtons)
                if (pad.IsButtonDown(b) && !_previousGamePadState.IsButtonDown(b)) return true;
        }
        return false;
    }

    public static bool IsActionReleased(InputAction action)
    {
        foreach (var key in action.KeyboardKeys)
            if (!_currentKeyboard.IsKeyDown(key) && _previousKeyboard.IsKeyDown(key)) return true;

        GamePadState pad = GamePad.GetState(PlayerIndex.One);
        if (pad.IsConnected)
        {
            foreach (var b in action.GamepadButtons)
                if (!pad.IsButtonDown(b) && _previousGamePadState.IsButtonDown(b)) return true;
        }
        return false;
    }

    public static void RemapKeyboard(string actionName, Keys newKey)
    {
        if (_actions.TryGetValue(actionName, out var a))
        {
            a.KeyboardKeys.Clear();
            a.AddKeyboardKey(newKey);
        }
    }

    // Mouse helpers
    public static Vector2 MousePosition => new Vector2(_currentMouse.X, _currentMouse.Y);
    public static bool LeftClick => _currentMouse.LeftButton == ButtonState.Pressed;
    public static bool LeftPressed => _currentMouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released;
    public static bool LeftReleased => _currentMouse.LeftButton == ButtonState.Released && _previousMouse.LeftButton == ButtonState.Pressed;
    public static bool RightClick => _currentMouse.RightButton == ButtonState.Pressed;
    public static int ScrollWheelDelta => _currentMouse.ScrollWheelValue - _previousMouse.ScrollWheelValue;
}