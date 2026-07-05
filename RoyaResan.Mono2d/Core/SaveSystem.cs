using System.Text.Json;

namespace RoyaResan.Mono2d.Core;

/// <summary>
/// Generic JSON save/load to a slot file. Engine-agnostic on purpose - it
/// doesn't know or care what a "save" contains. Each game defines its own
/// plain data class (player position, health, inventory, current level,
/// whatever) and passes it through Save/Load, same convention as
/// everywhere else in this framework: the engine gives you the mechanism,
/// game code decides the content. See SaveExample.cs.
/// </summary>
public static class SaveSystem
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    /// <summary>
    /// Where slot files are written. Defaults to a folder under the OS's
    /// per-user app data directory so saves survive reinstalls/updates and
    /// don't need write access next to the executable. Set this once at
    /// startup to your game's own folder name - e.g.
    /// SaveSystem.SaveDirectory = Path.Combine(Environment.GetFolderPath(
    ///     Environment.SpecialFolder.ApplicationData), "ShadowDancer");
    /// </summary>
    public static string SaveDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RoyaResanGame");

    public static void Save<T>(string slotName, T data)
    {
        Directory.CreateDirectory(SaveDirectory);
        string json = JsonSerializer.Serialize(data, Options);
        File.WriteAllText(GetPath(slotName), json);
    }

    /// <summary>Returns default(T) (null for reference types) if the slot doesn't exist yet - always check SlotExists first if that distinction matters to you.</summary>
    public static T? Load<T>(string slotName)
    {
        string path = GetPath(slotName);
        if (!File.Exists(path))
            return default;

        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(json, Options);
    }

    public static bool SlotExists(string slotName) => File.Exists(GetPath(slotName));

    public static void DeleteSlot(string slotName)
    {
        string path = GetPath(slotName);
        if (File.Exists(path))
            File.Delete(path);
    }

    private static string GetPath(string slotName) => Path.Combine(SaveDirectory, slotName + ".json");
}