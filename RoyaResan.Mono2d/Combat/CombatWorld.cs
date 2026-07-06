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

    /// <summary>Fires whenever a hit is stopped by BlockedTags (e.g. a raised shield) - deals no damage, but the attack is still "resolved" against this target (see the MarkHit call below), which is what makes a projectile despawn against a shield instead of piercing through to whoever's standing behind it.</summary>
    public event Action<Hitbox, Hurtbox> OnBlocked;

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

                // ---- FIX: BlockedTags check now runs AFTER the overlap test ----
                if (hurtbox.BlockedTags != null && hitbox.Tag != null && hurtbox.BlockedTags.Contains(hitbox.Tag))
                {
                    // Blocked (e.g. shield up) - no damage, but still mark
                    // it as resolved against THIS target. Without this, a
                    // projectile's HasHitAnyTarget never becomes true when
                    // it's blocked, so it just keeps flying at full speed
                    // and can go on to hit whoever is standing behind the
                    // shield in the same frame or the next one.
                    hitbox.MarkHit(hurtbox);
                    OnBlocked?.Invoke(hitbox, hurtbox);
                    continue;
                }

                // If not blocked, apply hit effects
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
                    ApplyKnockback(hitbox, hurtbox);
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

    /// <summary>
    /// Pushes the defender away from the attacker (Hitbox.Knockback) and
    /// optionally recoils the attacker the opposite way
    /// (Hitbox.SelfKnockback). Direction is derived from relative X
    /// position at the moment of the hit, not facing, so it's correct
    /// even for an omnidirectional or off-center hitbox. Sets velocity
    /// directly rather than adding to it, so knockback is a predictable,
    /// repeatable per-hit value regardless of whatever the target's
    /// velocity happened to be a frame ago.
    /// </summary>
    private static void ApplyKnockback(Hitbox hitbox, Hurtbox hurtbox)
    {
        if (hitbox.Owner == null || hurtbox.Owner == null)
            return;

        float dir = hurtbox.Owner.GlobalPosition.X >= hitbox.Owner.GlobalPosition.X ? 1f : -1f;

        if (hitbox.Knockback.X != 0f)
            hurtbox.Owner.Velocity.X = dir * hitbox.Knockback.X;
        if (hitbox.Knockback.Y != 0f)
            hurtbox.Owner.Velocity.Y = hitbox.Knockback.Y;

        if (hitbox.SelfKnockback.X != 0f)
            hitbox.Owner.Velocity.X = -dir * hitbox.SelfKnockback.X;
        if (hitbox.SelfKnockback.Y != 0f)
            hitbox.Owner.Velocity.Y = hitbox.SelfKnockback.Y;
    }
}