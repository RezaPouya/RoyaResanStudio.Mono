using RoyaResan.Mono2d.Graphics;
using RoyaResan.Mono2d.Physics;

namespace RoyaResan.Mono2d.Nodes;

/// <summary>
/// Renders one Tiled tile layer from a single tileset texture. Pure
/// visual - no collision. Built by LevelLoader, one per tile layer in the
/// map; add solid geometry separately via an object layer (see
/// LevelLoader's default "Wall"/"OneWayPlatform" spawners).
///
/// Draws every non-empty cell every frame with no culling - fine for
/// normal level sizes at 60fps with SpriteBatch, but if a map gets huge
/// this is the first place to add camera-rect culling.
/// </summary>
public class TileLayerNode : TransformNode
{
    public Texture2D Tileset = null!;
    public int TilesetColumns;
    public int TilesetFirstGid = 1;
    public int TileWidth;
    public int TileHeight;
    public int MapWidth;
    public int[] Tiles = Array.Empty<int>();

    public override void Draw(Renderer renderer)
    {
        for (int index = 0; index < Tiles.Length; index++)
        {
            int gid = Tiles[index];
            if (gid == 0)
                continue; // empty cell

            int col = index % MapWidth;
            int row = index / MapWidth;

            Vector2 worldPos = GlobalPosition + new Vector2(col * TileWidth, row * TileHeight);

            int tileIndex = gid - TilesetFirstGid;
            int srcCol = tileIndex % TilesetColumns;
            int srcRow = tileIndex / TilesetColumns;
            var sourceRect = new Rectangle(srcCol * TileWidth, srcRow * TileHeight, TileWidth, TileHeight);

            renderer.DrawTexture(Tileset, sourceRect, worldPos, Color.White);
        }

        base.Draw(renderer);
    }
}
