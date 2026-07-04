using System;

namespace RoyaResan.Mono2d.Combat;

/// <summary>
/// The region that can be hit. Carries a reference to a Health component
/// and the parry state: while IsParrying is true, an incoming hit is
/// deflected (OnParried fires, no damage) instead of dealing damage.
///
/// IsParrying is meant to be toggled by an attack/defense script reading
/// the owner's Animator - e.g. true only during a tight "active" window
/// inside a Parry animation state, false otherwise.
/// </summary>
public class Hurtbox
{
    public PhysicsBody Owner;
    public Vector2 Offset;
    public Vector2 Size;

    public Health Health;

    public bool IsParrying;

    /// <summary>
    /// While true, this hurtbox is skipped entirely by CombatWorld.Step -
    /// no damage, no parry, nothing (i-frames). Distinct from IsParrying:
    /// a parry can still be countered/read by other systems, invulnerable
    /// means "not there" as far as combat is concerned. Meant to be driven
    /// by a HurtboxProfile (e.g. true during a roll's active frames), same
    /// pattern as IsParrying being driven by ParryScript.
    /// </summary>
    public bool Invulnerable;

    /// <summary>Fires on the defender when they successfully parry an incoming hitbox.</summary>
    public event Action<Hitbox> OnParried;

    internal void RaiseParried(Hitbox attacker) => OnParried?.Invoke(attacker);

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
