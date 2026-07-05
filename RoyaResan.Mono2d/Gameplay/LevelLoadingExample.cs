using RoyaResan.Mono2d.Core;
using RoyaResan.Mono2d.Tilemap;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// USAGE EXAMPLE - not wired into World.cs automatically. Shows the whole
/// level-loading flow: parse a Tiled JSON export, hand it to LevelLoader
/// (which builds tile visuals + Wall/OneWayPlatform colliders), then read
/// back the "PlayerStart" marker object to place the player.
///
/// Workflow this assumes:
/// 1. Build the map in Tiled (mapeditor.org), one embedded tileset.
/// 2. File -> Export As -> level1.json, next to your other Content.
/// 3. In Tiled, add an object (any layer) with Type = "PlayerStart" where
///    the player should spawn. Add more typed objects (e.g. "EnemySpawn")
///    the same way - they'll show up in LevelResult.UnhandledObjects.
/// 4. Mark the .json (and tileset image) "Copy to Output Directory" in
///    your project - these are read directly from disk, not through the
///    MonoGame content pipeline, since they're just data.
/// </summary>
public static class LevelLoadingExample
{
    public static void LoadLevel1(Scene scene, string contentRoot, Func<string, Texture2D> loadTexture)
    {
        var map = TiledJsonLoader.Load(Path.Combine(contentRoot, "Levels", "level1.json"));

        // TilesetImagePath is whatever path Tiled wrote into the map JSON
        // (usually relative to the map file) - point loadTexture at wherever
        // that image actually lives in your project.
        var tilesetTexture = loadTexture(Path.Combine(contentRoot, "Levels", map.TilesetImagePath));

        var result = LevelLoader.Load(scene, map, tilesetTexture);

        var playerStart = result.FindSpawn("PlayerStart");
        Vector2 spawnPos = playerStart?.Center ?? Vector2.Zero;

        var player = new PhysicsBody { Position = spawnPos };
        player.Collider = new Collider { Owner = player, Size = new Vector2(32, 32) };
        player.AddScript(new PlatformerMovementScript());
        scene.AddBody(player);
        scene.Camera.FollowTarget = player;

        // Anything else typed in Tiled (EnemySpawn, etc.) is sitting right
        // here, unhandled on purpose - loop it and instantiate your own
        // enemy/trigger types the same way the player was just built.
        foreach (var obj in result.UnhandledObjects)
        {
            if (obj.Type == "EnemySpawn")
            {
                // e.g.: SpawnEnemy(scene, obj.Center, obj.Properties["enemyKind"]);
            }
        }
    }
}