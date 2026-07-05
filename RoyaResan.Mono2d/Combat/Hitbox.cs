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

    /// <summary>Optional free-form label ("Sword", "Kunai", ...) - lets a Hurtbox block by attack type via BlockedTags instead of just on/off (see the Shield enemy's directional block).</summary>
    public string Tag;

    /// <summary>Only checked for overlap while true - set by the attack script during active frames.</summary>
    public bool Active;

    // Prevents one active swing from hitting the same target every frame
    // it stays overlapping - cleared each time the swing (re)starts.
    private readonly HashSet<Hurtbox> _hitThisSwing = new();

    /// <summary>True if this hitbox has connected with anything since the last BeginSwing(). Handy for one-shot hits like a projectile that should despawn on its first hit, rather than enumerating _hitThisSwing.</summary>
    public bool HasHitAnyTarget { get; private set; }

    public void BeginSwing()
    {
        _hitThisSwing.Clear();
        HasHitAnyTarget = false;
    }

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