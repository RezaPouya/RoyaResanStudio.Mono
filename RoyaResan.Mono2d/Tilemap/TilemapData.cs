namespace RoyaResan.Mono2d.Tilemap;

/// <summary>
/// Parsed contents of a Tiled (mapeditor.org) JSON map export - Tiled was
/// chosen as the level format instead of a hand-rolled editor: it's free,
/// mature, and a solo dev doesn't have to build/maintain tooling for it.
/// This is pure data; TiledJsonLoader fills it, LevelLoader turns it into
/// real scene content.
///
/// Deliberately supports a SINGLE tileset per map (first tileset in the
/// file is used for every tile layer). Multi-tileset maps aren't handled -
/// keep one tileset image per map and this is a non-issue.
/// </summary>
public class TilemapData
{
    public int Width;
    public int Height;
    public int TileWidth;
    public int TileHeight;

    /// <summary>Tileset image path as written in the Tiled file (relative to the map) - caller loads the actual Texture2D.</summary>
    public string TilesetImagePath = "";
    public int TilesetColumns;
    public int TilesetFirstGid = 1;

    public List<TileLayerData> TileLayers = new();
    public List<ObjectLayerData> ObjectLayers = new();
}

/// <summary>One tile layer: a flat, row-major array of GIDs (0 = empty cell).</summary>
public class TileLayerData
{
    public string Name = "";
    public int[] Tiles = Array.Empty<int>();
}

/// <summary>One object layer (Tiled "objectgroup") - used for colliders, spawn points, triggers, anything non-visual.</summary>
public class ObjectLayerData
{
    public string Name = "";
    public List<TilemapObject> Objects = new();
}

/// <summary>
/// A single Tiled object. X/Y/Width/Height are in world pixels, top-left
/// origin (Tiled convention) - LevelLoader/game code converts to whatever
/// origin convention the target (Collider = center) needs.
/// </summary>
public class TilemapObject
{
    public string Name = "";
    public string Type = "";
    public float X, Y, Width, Height;
    public Dictionary<string, string> Properties = new();

    public Vector2 TopLeft => new(X, Y);
    public Vector2 Center => new(X + Width / 2f, Y + Height / 2f);
}
