namespace RoyaResan.Mono2d.Combat;

/// <summary>
/// An attack region attached to a PhysicsBody. Deliberately NOT the same
/// as Physics.Collider - a hitbox never pushes anything apart, it only
/// detects overlap with a Hurtbox. Gameplay code (an attack script, driven
/// by the Animator) turns Active on/off for the attack's active frames.
/// </summary>
public class Hitbox
{
    public PhysicsBody Owner;

    /// <summary>Offset from Owner.GlobalPosition, so it can sit in front of/around the entity.</summary>
    public Vector2 Offset;

    public Vector2 Size;

    public int Damage = 10;

    /// <summary>
    /// Horizontal magnitude (X) and vertical pop (Y) applied to the
    /// defender's velocity on a successful (non-blocked, non-parried)
    /// hit. X is a magnitude, not a signed direction - CombatWorld works
    /// out which way to push based on attacker/defender relative
    /// position, so you don't need to know facing when setting this.
    /// Y is applied as-is (negative = upward pop); leave at 0 for a
    /// purely horizontal shove. Zero (default) = no knockback at all,
    /// so existing hitboxes are unaffected unless you opt in.
    /// Set per instance, so a sword can knock back harder than a kunai,
    /// a heavy enemy's attack harder than a light one, etc.
    /// </summary>
    public Vector2 Knockback;

    /// <summary>Same shape as Knockback, but applied to the ATTACKER (recoil) instead of the defender - pushed the opposite direction. 0 (default) = no recoil.</summary>
    public Vector2 SelfKnockback;

    /// <summary>Optional free-form label ("Sword", "Kunai", ...) - lets a Hurtbox block by attack type via BlockedTags instead of just on/off (see the Shield enemy's directional block).</summary>
    public string Tag;

    /// <summary>Only checked for overlap while true - set by the attack script during active frames.</summary>
    public bool Active;

    /// <summary>
    /// Center this Hitbox was at the last time CombatWorld.Step processed
    /// it, used to sweep the segment between frames instead of only
    /// testing the current position - see CombatWorld.Step. Reset by
    /// BeginSwing() so a (re)armed hitbox never sweeps in from a stale
    /// position left over from a previous swing.
    /// </summary>
    public Vector2 PreviousCenter { get; private set; }

    /// <summary>
    /// Safety cap on the swept test: if the hitbox's center moved further
    /// than this in a single step, treat it as a teleport (e.g. Offset
    /// flipping when facing reverses mid-swing) and fall back to a plain
    /// point check at the current position rather than sweeping a long,
    /// spurious line across everything in between.
    /// </summary>
    public float MaxSweepDistance = 400f;

    // Prevents one active swing from hitting the same target every frame
    // it stays overlapping - cleared each time the swing (re)starts.
    private readonly HashSet<Hurtbox> _hitThisSwing = new();

    /// <summary>True if this hitbox has connected with anything since the last BeginSwing(). Handy for one-shot hits like a projectile that should despawn on its first hit, rather than enumerating _hitThisSwing.</summary>
    public bool HasHitAnyTarget { get; private set; }

    public void BeginSwing()
    {
        _hitThisSwing.Clear();
        HasHitAnyTarget = false;
        PreviousCenter = GetCenter();
    }

    /// <summary>Called by CombatWorld once per step after resolving this hitbox, so next step's sweep starts from here.</summary>
    public void CommitPreviousCenter() => PreviousCenter = GetCenter();

    public Vector2 GetCenter() => Owner != null ? Owner.GlobalPosition + Offset : Vector2.Zero;

    public bool HasHit(Hurtbox target) => _hitThisSwing.Contains(target);

    public void MarkHit(Hurtbox target)
    {
        _hitThisSwing.Add(target);
        HasHitAnyTarget = true;
    }

    public Rectangle GetBounds()
    {
        if (Owner == null)
            return Rectangle.Empty;

        Vector2 pos = Owner.GlobalPosition + Offset;
        return new Rectangle(
            (int)(pos.X - Size.X / 2f), (int)(pos.Y - Size.Y / 2f),
            (int)Size.X, (int)Size.Y);
    }
}