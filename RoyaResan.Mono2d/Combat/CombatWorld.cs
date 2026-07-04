using System;

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
                continue;

            var hitboxBounds = hitbox.GetBounds();

            foreach (var hurtbox in Hurtboxes)
            {
                if (hurtbox.Owner == null || hurtbox.Owner == hitbox.Owner)
                    continue; // no self-hit

                if (hurtbox.Invulnerable)
                    continue; // i-frames - e.g. mid-roll, per HurtboxProfile

                if (hitbox.HasHit(hurtbox))
                    continue; // already resolved this swing

                if (!hitboxBounds.Intersects(hurtbox.GetBounds()))
                    continue;

                hitbox.MarkHit(hurtbox);

                if (hurtbox.IsParrying)
                {
                    hurtbox.RaiseParried(hitbox);
                    OnParry?.Invoke(hitbox, hurtbox);
                }
                else
                {
                    hurtbox.Health?.Damage(hitbox.Damage, hitbox.Owner);
                    OnHit?.Invoke(hitbox, hurtbox);
                }
            }
        }
    }
}
