using System.Text.Json;

namespace RoyaResan.Mono2d.Tilemap;

/// <summary>
/// Parses a Tiled JSON map export (File -> Export As -> .json in Tiled)
/// into TilemapData. Only reads the subset Tiled always writes for
/// orthogonal maps with CSV/array tile data - no base64/zlib tile
/// compression support (Tiled's default export is plain arrays, so this
/// covers the normal workflow; if you ever enable compression in Tiled,
/// turn it back off).
///
/// GIDs with Tiled's flip flags (horizontal/vertical/diagonal, top 3 bits)
/// are masked back to a plain tile index - flipped tiles will render as
/// their unflipped version rather than crash. Actual flip rendering isn't
/// implemented; avoid using Tiled's flip/rotate stamp tools for now.
/// </summary>
public static class TiledJsonLoader
{
    private const uint FlipMask = 0xE0000000;

    public static TilemapData Load(string path)
    {
        string json = File.ReadAllText(path);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var map = new TilemapData
        {
            Width = root.GetProperty("width").GetInt32(),
            Height = root.GetProperty("height").GetInt32(),
            TileWidth = root.GetProperty("tilewidth").GetInt32(),
            TileHeight = root.GetProperty("tileheight").GetInt32(),
        };

        if (root.TryGetProperty("tilesets", out var tilesets) && tilesets.GetArrayLength() > 0)
        {
            var firstTileset = tilesets[0];
            map.TilesetFirstGid = firstTileset.GetProperty("firstgid").GetInt32();

            // External tileset files (.tsx, referenced via "source") aren't
            // followed - embed the tileset in the map JSON in Tiled instead.
            if (firstTileset.TryGetProperty("image", out var image))
                map.TilesetImagePath = image.GetString() ?? "";

            if (firstTileset.TryGetProperty("columns", out var columns))
                map.TilesetColumns = columns.GetInt32();
        }

        foreach (var layer in root.GetProperty("layers").EnumerateArray())
        {
            string type = layer.GetProperty("type").GetString() ?? "";
            string name = layer.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";

            if (type == "tilelayer")
            {
                var data = layer.GetProperty("data");
                var tiles = new int[data.GetArrayLength()];
                int i = 0;
                foreach (var cell in data.EnumerateArray())
                {
                    uint gid = cell.GetUInt32();
                    tiles[i++] = (int)(gid & ~FlipMask);
                }

                map.TileLayers.Add(new TileLayerData { Name = name, Tiles = tiles });
            }
            else if (type == "objectgroup")
            {
                var objLayer = new ObjectLayerData { Name = name };

                if (layer.TryGetProperty("objects", out var objects))
                {
                    foreach (var obj in objects.EnumerateArray())
                    {
                        var tilemapObj = new TilemapObject
                        {
                            Name = obj.TryGetProperty("name", out var on) ? on.GetString() ?? "" : "",
                            Type = obj.TryGetProperty("type", out var ot) ? ot.GetString() ?? "" : "",
                            X = obj.GetProperty("x").GetSingle(),
                            Y = obj.GetProperty("y").GetSingle(),
                            Width = obj.TryGetProperty("width", out var w) ? w.GetSingle() : 0f,
                            Height = obj.TryGetProperty("height", out var h) ? h.GetSingle() : 0f,
                        };

                        if (obj.TryGetProperty("properties", out var props))
                        {
                            foreach (var prop in props.EnumerateArray())
                            {
                                string propName = prop.GetProperty("name").GetString() ?? "";
                                // Read every property value as its raw JSON text - keeps this
                                // loader type-agnostic (int/float/bool/string all become a
                                // string here); game code parses to whatever type it expects.
                                string propValue = prop.GetProperty("value").ToString();
                                tilemapObj.Properties[propName] = propValue;
                            }
                        }

                        objLayer.Objects.Add(tilemapObj);
                    }
                }

                map.ObjectLayers.Add(objLayer);
            }
            // Other layer types (imagelayer, group) are silently skipped -
            // not needed for gameplay content.
        }

        return map;
    }
}