using RoyaResan.Mono2d.Animation;
using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// Picks which Animator state the player should be in this frame, from
/// PlatformerMovementScript + the attack/throw scripts - the "what state
/// are we actually in" decision that AnimationTransition's Condition
/// funcs would otherwise need to duplicate. Kept as a single script
/// (rather than a pile of AddTransition calls in World.cs) so the
/// priority order (Dead > Attack/Throw > airborne > grounded) is one
/// readable list instead of implicit ordering between separate rules.
///
/// Must be added to the player AFTER PlatformerMovementScript / the
/// attack and throw scripts, since PhysicsBody runs scripts in the order
/// they were added and this one needs to read their state as it is this
/// frame, not last frame.
/// </summary>
public class PlayerAnimatorDriverScript : Script
{
    public Animator Animator;
    public PlatformerMovementScript Movement;
    public SwordAttackScript Sword;
    public KunaiThrowScript Kunai;

    /// <summary>Set true (e.g. from Health.OnDeath) to lock the animator in "Dead" and stop this script from overriding it.</summary>
    public bool IsDead;

    public override void Update(GameTime gameTime)
    {
        if (IsDead)
        {
            Animator.Play("Dead", 0.1f);
            return;
        }

        if (Sword != null && Sword.IsAttacking)
        {
            Animator.Play("Attack", 0.05f);
            return;
        }

        if (Kunai != null && Kunai.IsThrowing)
        {
            Animator.Play("Throw", 0.05f);
            return;
        }

        if (!Owner.IsGrounded)
        {
            Animator.Play(Owner.Velocity.Y < 0f ? "Jump" : "Fall", 0.05f);
            return;
        }

        if (Movement.IsCrouching)
        {
            Animator.Play("Crouch", 0.05f);
            return;
        }

        Animator.Play(Math.Abs(Owner.Velocity.X) > 5f ? "Run" : "Idle", 0.05f);
    }
}
