namespace RoyaResan.Mono2d.Combat;

/// <summary>
/// A snapshot of a Hurtbox's shape/state for one Animator state - e.g.
/// "Crouch" shrinks Offset/Size, "Roll" sets Invulnerable = true. Applied
/// live by HurtboxProfileScript, which reads the owner's Animator and
/// swaps profiles as the current state changes. Pure data, no behavior.
/// </summary>
public struct HurtboxProfile
{
    public Vector2 Offset;
    public Vector2 Size;
    public bool Invulnerable;

    public HurtboxProfile(Vector2 offset, Vector2 size, bool invulnerable = false)
    {
        Offset = offset;
        Size = size;
        Invulnerable = invulnerable;
    }
}