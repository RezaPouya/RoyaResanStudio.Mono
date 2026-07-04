
using RoyaResan.Mono2d.Core;
using RoyaResan.Mono2d.UI;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// USAGE EXAMPLE - pause toggle + a real pause menu built from UiButton.
/// Call PauseExample.Setup once (e.g. at game startup) and PauseExample.Update
/// once per frame instead of calling scene.Update directly.
/// </summary>
public static class PauseExample
{
    private static UiElement _pauseMenu = null!;

    public static void Setup(Scene scene, SpriteFont font)
    {
        _pauseMenu = new UiElement { Position = new Vector2(300, 200), Size = new Vector2(200, 160) };

        _pauseMenu.AddChild(new UiButton
        {
            Position = new Vector2(0, 0),
            Size = new Vector2(200, 50),
            Font = font,
            Text = "Resume",
            OnClick = () => scene.IsPaused = false,
        });

        _pauseMenu.AddChild(new UiButton
        {
            Position = new Vector2(0, 60),
            Size = new Vector2(200, 50),
            Font = font,
            Text = "Save & Quit",
            OnClick = () =>
            {
                // e.g. SaveExample.Save(...) - see the save system
                scene.IsPaused = false;
            },
        });
    }

    public static void Update(Scene scene, GameTime gameTime)
    {
        if (Input.IsKeyPressed(Keys.Escape))
        {
            scene.IsPaused = !scene.IsPaused;

            if (scene.IsPaused)
                scene.Ui.Push(_pauseMenu);
            else
                scene.Ui.Pop();
        }

        // Scene.Update no-ops gameplay while paused but always runs UI,
        // so the menu above stays clickable.
        scene.Update(gameTime);
    }
}
