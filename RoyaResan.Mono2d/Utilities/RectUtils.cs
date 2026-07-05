namespace RoyaResan.Mono2d.Utilities;

public static class RectUtils
{
    public static bool Intersects(Rectangle a, Rectangle b)
        => a.Intersects(b);

    public static Rectangle Offset(Rectangle r, Vector2 offset)
        => new Rectangle(
            r.X + (int)offset.X,
            r.Y + (int)offset.Y,
            r.Width,
            r.Height
        );
}