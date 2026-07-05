using RoyaResan.Mono2d.Animation;
using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// Drives a Hurtbox's Offset/Size/Invulnerable from the owner's current
/// Animator state - the "shrink while crouching / no hitbox while rolling"
/// feature. Same pattern as ParryScript reading Animator.CurrentStateName
/// as the single source of truth, just applied to shape instead of a bool.
///
/// Usage:
///   var script = new HurtboxProfileScript
///   {
///       Hurtbox = hurtbox,
///       Animator = animator,
///       Default = new HurtboxProfile(Vector2.Zero, new Vector2(32, 48)),
///   };
///   script.States["Crouch"] = new HurtboxProfile(new Vector2(0, 8), new Vector2(32, 28));
///   script.States["Roll"]   = new HurtboxProfile(Vector2.Zero, Vector2.Zero, invulnerable: true);
///   body.AddScript(script);
///
/// Any Animator state not present in States falls back to Default, so you
/// only need to list the states that actually change the hurtbox.
/// </summary>
public class HurtboxProfileScript : Script
{
    public Hurtbox Hurtbox;
    public Animator Animator;

    public HurtboxProfile Default;
    public readonly Dictionary<string, HurtboxProfile> States = new();

    public override void Update(GameTime gameTime)
    {
        var profile = States.TryGetValue(Animator.CurrentStateName, out var p) ? p : Default;

        Hurtbox.Offset = profile.Offset;
        Hurtbox.Size = profile.Size;
        Hurtbox.Invulnerable = profile.Invulnerable;
    }
}