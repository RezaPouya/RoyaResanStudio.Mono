using RoyaResan.Mono2d.Core;
using RoyaResan.Mono2d.Physics;
using RoyaResan.Mono2d.Combat;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// USAGE EXAMPLE - your own save data shape, and where in your game code
/// you'd read/write it. SaveSystem itself has no idea what a "save" is;
/// this class is what makes it one for a specific game.
/// </summary>
public class GameSaveData
{
    public float PlayerX;
    public float PlayerY;
    public int PlayerHealth;
    public string CurrentLevel = "level1";
    public List<string> UnlockedAbilities = new();
}

public static class SaveExample
{
    private const string SlotName = "slot1";

    public static void Save(PhysicsBody player, Health playerHealth, string currentLevel)
    {
        var data = new GameSaveData
        {
            PlayerX = player.Position.X,
            PlayerY = player.Position.Y,
            PlayerHealth = playerHealth.Current,
            CurrentLevel = currentLevel,
        };

        SaveSystem.Save(SlotName, data);
    }

    /// <summary>Returns false (and leaves the player untouched) if there's no save yet - call SlotExists first if you need to show/hide a "Continue" button.</summary>
    public static bool Load(PhysicsBody player, Health playerHealth, out string currentLevel)
    {
        currentLevel = "level1";

        var data = SaveSystem.Load<GameSaveData>(SlotName);
        if (data == null)
            return false;

        player.Position = new Vector2(data.PlayerX, data.PlayerY);
        playerHealth.Current = data.PlayerHealth;
        currentLevel = data.CurrentLevel;
        return true;
    }
}
