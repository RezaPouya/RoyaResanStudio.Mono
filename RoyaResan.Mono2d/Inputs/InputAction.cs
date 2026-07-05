namespace RoyaResan.Mono2d.Inputs;

public class InputAction
{
    public string Name { get; }
    public List<Keys> KeyboardKeys { get; } = new();
    public List<Buttons> GamepadButtons { get; } = new();

    public InputAction(string name) => Name = name;

    public void AddKeyboardKey(Keys key) => KeyboardKeys.Add(key);
    public void AddGamepadButton(Buttons button) => GamepadButtons.Add(button);
}
