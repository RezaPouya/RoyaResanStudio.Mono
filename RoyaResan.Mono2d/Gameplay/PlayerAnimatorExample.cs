using RoyaResan.Mono2d.Animation;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// USAGE EXAMPLE - not wired into World.cs automatically. Shows how to
/// build an Animator for a player using ONE shared spritesheet, with
/// state-based speed and blended transitions into/out of an attack.
///
/// Layout assumption: spritesheet has rows of 32x32 frames -
/// row 0 = idle (4 frames), row 1 = run (6 frames), row 2 = attack (5 frames).
/// Adjust frameWidth/frameHeight/frameCount/columns/startY to your actual sheet.
/// </summary>
public static class PlayerAnimatorExample
{
    public static Animator Build(Texture2D spriteSheet)
    {
        var idle = AnimationClip.FromGrid("idle", spriteSheet,
            frameWidth: 32, frameHeight: 32, frameCount: 4, columns: 4,
            startX: 0, startY: 0, frameTime: 0.15f, loop: true);

        var run = AnimationClip.FromGrid("run", spriteSheet,
            frameWidth: 32, frameHeight: 32, frameCount: 6, columns: 6,
            startX: 0, startY: 32, frameTime: 0.08f, loop: true);

        var attack = AnimationClip.FromGrid("attack", spriteSheet,
            frameWidth: 32, frameHeight: 32, frameCount: 5, columns: 5,
            startX: 0, startY: 64, frameTime: 0.05f, loop: false);

        var animator = new Animator();

        animator.AddState(new AnimationState { Name = "Idle", Clip = idle, Speed = 1f });
        animator.AddState(new AnimationState { Name = "Run", Clip = run, Speed = 1f });
        animator.AddState(new AnimationState { Name = "Attack", Clip = attack, Speed = 1f });

        // Example condition hooks - replace with real gameplay state reads
        // (e.g. owner.Velocity, an "IsAttacking" flag set by a combat script).
        bool isMoving = false;
        bool isAttacking = false;

        animator.AddTransition(new AnimationTransition
        {
            From = "*",
            To = "Attack",
            Condition = () => isAttacking,
            BlendDuration = 0.05f // snappy blend into attack
        });

        animator.AddTransition(new AnimationTransition
        {
            From = "Attack",
            To = "Idle",
            Condition = () => !isAttacking && !isMoving,
            BlendDuration = 0.1f
        });

        animator.AddTransition(new AnimationTransition
        {
            From = "Idle",
            To = "Run",
            Condition = () => isMoving,
            BlendDuration = 0.1f
        });

        animator.AddTransition(new AnimationTransition
        {
            From = "Run",
            To = "Idle",
            Condition = () => !isMoving,
            BlendDuration = 0.1f
        });

        animator.Play("Idle");
        return animator;

        // Environment-based speed example (call from anywhere, anytime):
        //   animator.SpeedMultiplier = inWater ? 0.5f : (onIce ? 1.3f : 1f);
    }
}