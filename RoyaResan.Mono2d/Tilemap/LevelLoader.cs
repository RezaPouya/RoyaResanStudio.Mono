using RoyaResan.Mono2d.Core;

namespace RoyaResan.Mono2d.Tilemap;

/// <summary>
/// Turns a parsed TilemapData into real scene content: TileLayerNode(s)
/// for visuals, and PhysicsBody colliders for object-layer entries that
/// have a registered spawner (Wall/OneWayPlatform out of the box).
///
/// Object types with NO registered spawner (e.g. "PlayerStart",
/// "EnemySpawn") are intentionally NOT auto-instantiated - they come back
/// in LevelResult.UnhandledObjects for game code to wire up manually in
/// World.cs, same convention as everything else in this framework
/// (Scene/AI/Combat wiring all happens in game code, never automatically).
/// Register your own spawners via ObjectSpawners for anything you want
/// the loader to build directly (decorations, triggers, etc).
/// </summary>
public static class LevelLoader
{
    public delegate void ObjectSpawner(Scene scene, TilemapObject obj);

    /// <summary>Keyed by TilemapObject.Type (the "Class/Type" field set on the object in Tiled).</summary>
    public static readonly Dictionary<string, ObjectSpawner> ObjectSpawners = new()
    {
        ["Wall"] = SpawnWall,
        ["OneWayPlatform"] = SpawnOneWayPlatform,
    };

    public static LevelResult Load(Scene scene, TilemapData map, Texture2D tilesetTexture)
    {
        var result = new LevelResult();

        foreach (var layerData in map.TileLayers)
        {
            var layerNode = new TileLayerNode
            {
                Tileset = tilesetTexture,
                TilesetColumns = map.TilesetColumns,
                TilesetFirstGid = map.TilesetFirstGid,
                TileWidth = map.TileWidth,
                TileHeight = map.TileHeight,
                MapWidth = map.Width,
                Tiles = layerData.Tiles,
                Name = layerData.Name,
            };

            scene.Root.AddChild(layerNode);
            result.TileLayers.Add(layerNode);
        }

        foreach (var objLayer in map.ObjectLayers)
        {
            foreach (var obj in objLayer.Objects)
            {
                if (ObjectSpawners.TryGetValue(obj.Type, out var spawner))
                    spawner(scene, obj);
                else
                    result.UnhandledObjects.Add(obj);
            }
        }

        return result;
    }

    // Tiled objects are top-left-origin; Collider.Bounds is center-origin,
    // so every spawner below converts via obj.Center.

    private static void SpawnWall(Scene scene, TilemapObject obj)
    {
        var wall = new WallNode { Position = obj.Center };
        wall.Collider.Size = new Vector2(obj.Width, obj.Height);
        scene.AddBody(wall);
    }

    private static void SpawnOneWayPlatform(Scene scene, TilemapObject obj)
    {
        var platform = new OneWayPlatformNode { Position = obj.Center };
        platform.Collider.Size = new Vector2(obj.Width, obj.Height);
        scene.AddBody(platform);
    }
}

/// <summary>What LevelLoader.Load hands back - everything the loader didn't already wire into the scene itself.</summary>
public class LevelResult
{
    public List<TileLayerNode> TileLayers = new();

    /// <summary>Object-layer entries with no registered spawner - e.g. "PlayerStart", "EnemySpawn". Read obj.Type/obj.Name/obj.Properties and wire these yourself.</summary>
    public List<TilemapObject> UnhandledObjects = new();

    /// <summary>Convenience: first unhandled object of the given Type, or null. Common case: FindSpawn("PlayerStart").</summary>
    public TilemapObject? FindSpawn(string type) =>
        UnhandledObjects.Find(o => o.Type == type);
}