using RoyaResan.Mono2d.Animation;
using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// Front-facing melee attack: on AttackKey press (if off cooldown), plays
/// the Attack animation state and arms Hitbox for a short active window,
/// positioned in front of the owner based on Movement.FacingRight.
///
/// The Hitbox itself is NOT created here - build one and register it with
/// Scene.AddHitbox once at setup (same convention as HurtboxProfileScript
/// takes an already-registered Hurtbox). This script only toggles
/// Active/Offset/Size on it, matching "engine gives mechanism, game code
/// wires content."
///
/// Usage:
///   var hitbox = new Hitbox { Owner = player, Damage = 1 };
///   scene.AddHitbox(hitbox);
///   var sword = new SwordAttackScript { Hitbox = hitbox, Movement = platformerScript, Animator = animator };
///   player.AddScript(sword);
/// </summary>
public class SwordAttackScript : Script
{
    public Hitbox Hitbox;
    public PlatformerMovementScript Movement;
    public Animator Animator;

    /// <summary>Full time before another attack can start - matches the design doc's 0.35s cadence.</summary>
    public float Cooldown = 0.35f;

    /// <summary>How long within that cooldown the hitbox is actually Active (the "active frames").</summary>
    public float ActiveDuration = 0.12f;

    public Vector2 ForwardOffset = new Vector2(10f, 0f);
    public Vector2 Size = new Vector2(28f, 20f);

    /// <summary>True for the whole attack pose duration (ActiveDuration), not just while Hitbox.Active - lets an Animator transition rule yield to the Attack state while this is true and resume normal movement states once it's false.</summary>
    public bool IsAttacking { get; private set; }

    private float _cooldownTimer;
    private float _activeTimer;

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_cooldownTimer > 0f)
            _cooldownTimer -= dt;

        if (!IsAttacking && _cooldownTimer <= 0f && InputManager.IsActionPressed(InputManager.Attack))
        {
            IsAttacking = true;
            _activeTimer = ActiveDuration;
            _cooldownTimer = Cooldown;

            Hitbox.BeginSwing();
            Hitbox.Active = true;
            Animator?.Play("Attack", 0.05f);
        }

        if (IsAttacking)
        {
            bool facingRight = Movement?.FacingRight ?? true;
            Hitbox.Offset = new Vector2(facingRight ? ForwardOffset.X : -ForwardOffset.X, ForwardOffset.Y);
            Hitbox.Size = Size;

            _activeTimer -= dt;
            if (_activeTimer <= 0f)
            {
                Hitbox.Active = false;
                IsAttacking = false;
            }
        }
    }
}