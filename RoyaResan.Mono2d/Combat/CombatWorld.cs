namespace RoyaResan.Mono2d.Combat;

/// <summary>
/// Owns all Hitboxes/Hurtboxes in a scene and resolves overlaps each
/// frame - deliberately separate from PhysicsWorld (combat never causes
/// position separation, only damage/parry outcomes).
/// </summary>
public class CombatWorld
{
    public readonly List<Hitbox> Hitboxes = new();
    public readonly List<Hurtbox> Hurtboxes = new();

    /// <summary>Fires whenever a hit is deflected - hook camera shake/sound/stagger here.</summary>
    public event Action<Hitbox, Hurtbox> OnParry;

    /// <summary>Fires whenever a hit actually lands (not parried).</summary>
    public event Action<Hitbox, Hurtbox> OnHit;

    public void Step()
    {
        foreach (var hitbox in Hitboxes)
        {
            if (!hitbox.Active || hitbox.Owner == null)
            {
                // Keep an inactive hitbox's sweep anchor current so that
                // when it goes Active again without a fresh BeginSwing()
                // (shouldn't normally happen, but cheap insurance) it
                // doesn't sweep in from a long-stale position.
                if (hitbox.Owner != null)
                    hitbox.CommitPreviousCenter();
                continue;
            }

            Vector2 currentCenter = hitbox.GetCenter();
            Vector2 sweepStart = hitbox.PreviousCenter;

            // Guard against a huge, spurious sweep (e.g. Offset flipping
            // instantly when facing reverses mid-swing) - fall back to a
            // point check at the current position instead of drawing a
            // long line across everything in between.
            if (Vector2.Distance(sweepStart, currentCenter) > hitbox.MaxSweepDistance)
                sweepStart = currentCenter;

            foreach (var hurtbox in Hurtboxes)
            {
                if (hurtbox.Owner == null || hurtbox.Owner == hitbox.Owner)
                    continue; // no self-hit

                // Prevent friendly fire
                if (hitbox.Owner.Team == hurtbox.Owner.Team && hitbox.Owner.Team != "None")
                    continue;

                if (hurtbox.Invulnerable)
                    continue; // i-frames - e.g. mid-roll, per HurtboxProfile

                if (hurtbox.BlockedTags != null && hitbox.Tag != null && hurtbox.BlockedTags.Contains(hitbox.Tag))
                    continue; // blocked by attack type - e.g. a raised shield blocking Sword but not Kunai

                if (hitbox.HasHit(hurtbox))
                    continue; // already resolved this swing

                // Swept test: expand the hurtbox by the hitbox's own
                // half-size (Minkowski sum) and test the hitbox's full
                // path this step - not just its current position -
                // against that expanded rect. A stationary hitbox
                // (sweepStart == currentCenter) degenerates to exactly
                // the old point-in-rect check, so melee/idle cases behave
                // identically to before; only a moving hitbox (a
                // projectile) gets the extra continuous check.
                var expanded = ExpandByHalfSize(hurtbox.GetBounds(), hitbox.Size);

                if (!SweptCollision.SegmentIntersectsRect(sweepStart, currentCenter, expanded))
                    continue;

                hitbox.MarkHit(hurtbox);

                if (hurtbox.IsParrying)
                {
                    hurtbox.RaiseParried(hitbox);
                    OnParry?.Invoke(hitbox, hurtbox);
                }
                else
                {
                    // Use UserData if available (for projectiles), otherwise use hitbox.Owner
                    var damageSource = hitbox.Owner.UserData as PhysicsBody ?? hitbox.Owner;
                    hurtbox.Health?.Damage(hitbox.Damage, damageSource);
                    OnHit?.Invoke(hitbox, hurtbox);
                }
            }

            hitbox.CommitPreviousCenter();
        }
    }

    /// <summary>
    /// Grows `rect` outward by half of `size` on every side - the
    /// Minkowski sum used to turn "does a sized hitbox's path cross this
    /// rect" into the cheaper "does a point's path cross this bigger
    /// rect" for the swept test.
    /// </summary>
    private static Rectangle ExpandByHalfSize(Rectangle rect, Vector2 size)
    {
        int halfW = (int)(size.X / 2f);
        int halfH = (int)(size.Y / 2f);
        return new Rectangle(rect.X - halfW, rect.Y - halfH, rect.Width + halfW * 2, rect.Height + halfH * 2);
    }
}