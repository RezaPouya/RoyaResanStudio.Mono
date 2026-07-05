namespace RoyaResan.Mono2d.Physics;

public struct RaycastHit
{
    public PhysicsBody Body;
    public Vector2 Point;
    public float Distance;
}

/// <summary>
/// Raycast support. Two entry points:
///
/// - HasLineOfSight: boolean-only, static-geometry-only, used by vision
///   cones. Deliberately narrow - "is there a wall in the way".
///
/// - Cast: general-purpose - returns the CLOSEST hit (body, world point,
///   distance) among any bodies matching an optional filter, not limited
///   to static geometry. This is what a Bionic-Commando-style "fire the
///   rope wherever I'm aiming" needs that HasLineOfSight can't give you.
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

    /// <summary>
    /// Casts from origin toward direction for up to maxDistance. Returns
    /// true and fills hit with the closest matching body if anything was
    /// hit. filter defaults to "any body with a Collider" - pass e.g.
    /// b => b.IsStatic to only hit walls, ignoring the player/enemies.
    /// </summary>
    public static bool Cast(
        PhysicsWorld world, Vector2 origin, Vector2 direction, float maxDistance,
        out RaycastHit hit, Func<PhysicsBody, bool> filter = null, PhysicsBody ignore = null)
    {
        hit = default;

        if (direction.LengthSquared() < 0.0001f || maxDistance <= 0f)
            return false;

        direction.Normalize();
        Vector2 end = origin + direction * maxDistance;

        float closestDistSq = float.MaxValue;
        bool found = false;

        foreach (var body in world.Bodies)
        {
            if (body == ignore || body.Collider == null)
                continue;

            if (filter != null && !filter(body))
                continue;

            if (SegmentIntersectsRectWithPoint(origin, end, body.Collider.Bounds, out Vector2 point))
            {
                float distSq = Vector2.DistanceSquared(origin, point);
                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    hit = new RaycastHit
                    {
                        Body = body,
                        Point = point,
                        Distance = MathF.Sqrt(distSq)
                    };
                    found = true;
                }
            }
        }

        return found;
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

    /// <summary>Same edge-by-edge test as SegmentIntersectsRect, but also returns the closest intersection point (or the origin, if it starts inside the rect).</summary>
    private static bool SegmentIntersectsRectWithPoint(
      Vector2 p1, Vector2 p2, Rectangle rect, out Vector2 point)
    {
        Vector2 result = default;
        float closestDistSq = float.MaxValue;
        bool found = false;

        void TryCandidate(Vector2 candidate)
        {
            float d = Vector2.DistanceSquared(p1, candidate);
            if (d < closestDistSq)
            {
                closestDistSq = d;
                result = candidate;
                found = true;
            }
        }

        if (rect.Contains(p1.ToPoint()))
            TryCandidate(p1);

        Vector2 tl = new(rect.Left, rect.Top);
        Vector2 tr = new(rect.Right, rect.Top);
        Vector2 bl = new(rect.Left, rect.Bottom);
        Vector2 br = new(rect.Right, rect.Bottom);

        if (SegmentsIntersectPoint(p1, p2, tl, tr, out var pt1)) TryCandidate(pt1);
        if (SegmentsIntersectPoint(p1, p2, tr, br, out var pt2)) TryCandidate(pt2);
        if (SegmentsIntersectPoint(p1, p2, br, bl, out var pt3)) TryCandidate(pt3);
        if (SegmentsIntersectPoint(p1, p2, bl, tl, out var pt4)) TryCandidate(pt4);

        point = result;
        return found;
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

    /// <summary>Same test as SegmentsIntersect, but also computes the actual intersection point.</summary>
    private static bool SegmentsIntersectPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 point)
    {
        point = default;

        float d1 = Cross(d - c, a - c);
        float d2 = Cross(d - c, b - c);
        float d3 = Cross(b - a, c - a);
        float d4 = Cross(b - a, d - a);

        bool intersects = ((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                           ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0));

        if (!intersects)
            return false;

        float denom = (b.X - a.X) * (d.Y - c.Y) - (b.Y - a.Y) * (d.X - c.X);
        if (Math.Abs(denom) < 0.0001f)
            return false;

        float t = ((c.X - a.X) * (d.Y - c.Y) - (c.Y - a.Y) * (d.X - c.X)) / denom;
        point = a + t * (b - a);
        return true;
    }

    private static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
}