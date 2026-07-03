namespace RoyaResan.Mono2d.Physics;

/// <summary>
/// Minimal raycast support - the framework had none before this. Only
/// checks line-vs-rectangle against STATIC colliders (walls), since for
/// vision/stealth purposes only level geometry should block sight, not
/// other moving bodies. This is a segment-vs-AABB test, not a general
/// physics raycast (no distance-to-hit, no first-hit-wins ordering) -
/// enough for "is there a wall between A and B", which is what vision
/// cones and most gameplay raycast needs actually are.
/// </summary>
public static class Raycast
{
    public static bool HasLineOfSight(PhysicsWorld world, Vector2 from, Vector2 to, PhysicsBody ignoreA = null, PhysicsBody ignoreB = null)
    {
        foreach (var body in world.Bodies)
        {
            if (body == ignoreA || body == ignoreB)
                continue;

            if (body.Collider == null || !body.IsStatic)
                continue; // only solid static geometry blocks sight

            if (SegmentIntersectsRect(from, to, body.Collider.Bounds))
                return false;
        }

        return true;
    }

    private static bool SegmentIntersectsRect(Vector2 p1, Vector2 p2, Rectangle rect)
    {
        if (rect.Contains(p1.ToPoint()) || rect.Contains(p2.ToPoint()))
            return true;

        Vector2 tl = new Vector2(rect.Left, rect.Top);
        Vector2 tr = new Vector2(rect.Right, rect.Top);
        Vector2 bl = new Vector2(rect.Left, rect.Bottom);
        Vector2 br = new Vector2(rect.Right, rect.Bottom);

        return SegmentsIntersect(p1, p2, tl, tr) ||
               SegmentsIntersect(p1, p2, tr, br) ||
               SegmentsIntersect(p1, p2, br, bl) ||
               SegmentsIntersect(p1, p2, bl, tl);
    }

    private static bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        float d1 = Cross(d - c, a - c);
        float d2 = Cross(d - c, b - c);
        float d3 = Cross(b - a, c - a);
        float d4 = Cross(b - a, d - a);

        return ((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
               ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0));
    }

    private static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
}
