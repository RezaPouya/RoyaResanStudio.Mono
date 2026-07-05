namespace RoyaResan.Mono2d.Combat;

/// <summary>
/// Continuous (swept) collision test between a moving point and a static
/// rectangle, using the standard slab method. Used by CombatWorld to
/// check a Hitbox's full path between two frames instead of only its
/// current position - this is what prevents a fast Hitbox from tunneling
/// through a thin Hurtbox between simulation steps.
///
/// To account for the Hitbox's own size (not just its center point), the
/// caller expands the target rectangle by the Hitbox's half-extents
/// (Minkowski sum) before calling this - see CombatWorld.ExpandByHalfSize.
/// </summary>
public static class SweptCollision
{
    /// <summary>
    /// True if the segment from `start` to `end` passes through `rect`
    /// at any point, including if `start` already starts inside it (a
    /// stationary or zero-movement Hitbox degenerates to a plain
    /// point-in-rect test, i.e. the same result as the old discrete
    /// overlap check).
    /// </summary>
    public static bool SegmentIntersectsRect(Vector2 start, Vector2 end, Rectangle rect)
    {
        Vector2 dir = end - start;

        float tMin = 0f;
        float tMax = 1f;

        if (!ClipAxis(start.X, dir.X, rect.Left, rect.Right, ref tMin, ref tMax))
            return false;

        if (!ClipAxis(start.Y, dir.Y, rect.Top, rect.Bottom, ref tMin, ref tMax))
            return false;

        return tMin <= tMax;
    }

    private static bool ClipAxis(float p, float d, float lo, float hi, ref float tMin, ref float tMax)
    {
        const float epsilon = 1e-6f;

        if (MathF.Abs(d) < epsilon)
        {
            // Segment doesn't move along this axis - only intersects if
            // the (unmoving) coordinate already sits inside the slab.
            return p >= lo && p <= hi;
        }

        float t0 = (lo - p) / d;
        float t1 = (hi - p) / d;

        if (t0 > t1)
            (t0, t1) = (t1, t0);

        tMin = MathF.Max(tMin, t0);
        tMax = MathF.Min(tMax, t1);

        return tMin <= tMax;
    }
}
