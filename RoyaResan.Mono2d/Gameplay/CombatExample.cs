using RoyaResan.Mono2d.Animation;
using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Graphics;
using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// USAGE EXAMPLE - not wired into World.cs automatically. Shows the whole
/// combat loop: an attack script that arms the hitbox only during the
/// Animator's "Attack" state, a defender that opens a parry window during
/// its own "Parry" state, and CombatWorld events driving camera shake +
/// sound through ImpactFeedback.
/// </summary>
public class AttackScript : Script
{
    public Hitbox Hitbox;
    public Animator Animator;

    private bool _swingStarted;

    public override void Update(GameTime gameTime)
    {
        bool isAttacking = Animator.CurrentStateName == "Attack";

        // Arm the hitbox only while the attack animation is actually
        // playing - this IS the "active frames" window. For finer control
        // (e.g. only frames 2-4 of a 6-frame swing), gate this further by
        // reading Animator.CurrentFrame and checking its Rectangle against
        // known active-frame rects, or add a frame-index property to
        // Animator if you need that precision.
        if (isAttacking && !_swingStarted)
        {
            Hitbox.BeginSwing();
            _swingStarted = true;
        }
        else if (!isAttacking)
        {
            _swingStarted = false;
        }

        Hitbox.Active = isAttacking;
    }
}

public class ParryScript : Script
{
    public Hurtbox Hurtbox;
    public Animator Animator;

    public override void Update(GameTime gameTime)
    {
        // Parry is only active while the Parry animation state is
        // playing - a short, committed window, same idea as attack
        // active frames but on defense.
        Hurtbox.IsParrying = Animator.CurrentStateName == "Parry";
    }
}

public static class CombatWiringExample
{
    /// <summary>
    /// Wires CombatWorld events to camera shake + sound. Call once after
    /// building the scene. Parries get a stronger shake than plain hits -
    /// that asymmetry is what makes a successful parry feel rewarding.
    /// </summary>
    public static void WireFeedback(CombatWorld combat, Camera2D camera, SoundEffect hitSound, SoundEffect parrySound)
    {
        combat.OnHit += (hitbox, hurtbox) =>
            ImpactFeedback.Trigger(camera, hitSound, shakeDuration: 0.12f, shakeMagnitude: 5f);

        combat.OnParry += (hitbox, hurtbox) =>
            ImpactFeedback.Trigger(camera, parrySound, shakeDuration: 0.2f, shakeMagnitude: 9f, volume: 1.2f);
    }
}
