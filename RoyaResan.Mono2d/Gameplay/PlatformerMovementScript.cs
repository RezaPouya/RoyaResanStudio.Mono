using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// Side-scrolling platformer movement: horizontal run with accel/decel
/// (not an instant-speed snap like PlayerMovementScript), plus the jump
/// primitives that make a platformer actually feel good:
///
///   - Coyote time: jump still works for a few frames after walking off
///     a ledge, matching what the player's eye/thumb actually perceives.
///   - Jump buffering: pressing jump slightly before landing still fires
///     the jump the instant you land, instead of eating the input.
///   - Variable jump height: releasing jump early while still rising cuts
///     the jump short (tap = short hop, hold = full jump).
///   - Fall/low-jump gravity multipliers: extra gravity while falling (and
///     while rising without holding jump) for a snappier arc than uniform
///     gravity gives you - the standard "better jump" trick.
///
/// Sets Owner.UseGravity = true itself, so just add this script to any
/// PhysicsBody with a Collider and it behaves like a platformer character.
/// Grounding comes from PhysicsBody.IsGrounded, set by PhysicsWorld when
/// this body rests on a normal collider or gets caught by a one-way
/// platform - no ground-raycast needed.
///
/// This is a separate script from PlayerMovementScript on purpose: that
/// one is a top-down 4-directional mover used by existing stealth/combat
/// examples, and nothing here should risk changing its behavior.
/// </summary>
public class PlatformerMovementScript : Script
{
    public float MoveSpeed = 250f;
    public float Acceleration = 1600f;
    public float Deceleration = 2000f;

    /// <summary>Initial upward speed applied on jump (positive value; applied to Velocity.Y as negative).</summary>
    public float JumpVelocity = 580f;

    public float CoyoteTime = 0.1f;
    public float JumpBufferTime = 0.12f;

    /// <summary>Extra gravity multiplier while rising and jump is NOT held - lets a tapped jump cut short instead of always reaching full height.</summary>
    public float LowJumpGravityMultiplier = 2.5f;

    /// <summary>Extra gravity multiplier while falling - snappier descent than a symmetric arc.</summary>
    public float FallGravityMultiplier = 2.6f;



    /// <summary>True = facing right. Starts true. Updated whenever there's horizontal input; holds last direction while idle. Read by attack/kunai-aim scripts and the placeholder pose driver.</summary>
    public bool FacingRight { get; private set; } = true;

    /// <summary>True while grounded and holding CrouchKey. Doesn't change movement math itself - a crouch speed/hurtbox-shrink effect is layered on by whatever script cares (see HurtboxProfileScript / PlaceholderPoseScript).</summary>
    public bool IsCrouching { get; private set; }

    /// <summary>Hard cap on downward speed - without this, a long fall (e.g. off the level, or before a floor exists) can reach a velocity high enough to tunnel straight through a thin collider in one frame instead of landing on it.</summary>
    public float MaxFallSpeed = 1100f;

    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private bool _isJumpRising;

    public override void Start()
    {
        Owner.UseGravity = true;
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        UpdateHorizontal(dt);
        UpdateJump(dt);
    }

    private void UpdateHorizontal(float dt)
    {
        float inputX = 0f;
        if (InputManager.IsActionDown(InputManager.Left)) inputX -= 1f;
        if (InputManager.IsActionDown(InputManager.Right)) inputX += 1f;

        if (inputX > 0f) FacingRight = true;
        else if (inputX < 0f) FacingRight = false;

        IsCrouching = Owner.IsGrounded && InputManager.IsActionDown(InputManager.Crouch);

        float targetSpeed = inputX * MoveSpeed;
        float rate = Math.Abs(targetSpeed) > 0.01f ? Acceleration : Deceleration;

        Owner.Velocity.X = MoveToward(Owner.Velocity.X, targetSpeed, rate * dt);
    }

    private void UpdateJump(float dt)
    {
        // Coyote time: keep "still allowed to jump" true for a short
        // window after leaving the ground, refreshed every frame we're
        // actually grounded.
        _coyoteTimer = Owner.IsGrounded ? CoyoteTime : _coyoteTimer - dt;

        // Jump buffering: remember a jump press for a short window so it
        // still fires if it landed slightly before touching ground.
        if (InputManager.IsActionPressed(InputManager.Jump))
            _jumpBufferTimer = JumpBufferTime;
        else
            _jumpBufferTimer -= dt;

        if (_coyoteTimer > 0f && _jumpBufferTimer > 0f)
        {
            Owner.Velocity.Y = -JumpVelocity;
            _coyoteTimer = 0f;
            _jumpBufferTimer = 0f;
            _isJumpRising = true;
        }

        // Variable height: released early while still going up -> cut the
        // rise short right now instead of waiting for gravity to do it.
        if (_isJumpRising && InputManager.IsActionReleased(InputManager.Jump) && Owner.Velocity.Y < 0f)
            Owner.Velocity.Y *= 0.5f;

        if (Owner.Velocity.Y >= 0f)
            _isJumpRising = false; // apex passed or landed - no longer an active "held jump"

        // Extra gravity on top of PhysicsBody's base gravity integration
        // (which runs after scripts, later this same Update call) - this
        // is what actually shapes the fast-fall / short-hop arc.
        if (Owner.Velocity.Y > 0f)
            Owner.Velocity.Y += PhysicsSettings.Gravity * (FallGravityMultiplier - 1f) * dt;
        else if (Owner.Velocity.Y < 0f && !InputManager.IsActionDown(InputManager.Jump))
            Owner.Velocity.Y += PhysicsSettings.Gravity * (LowJumpGravityMultiplier - 1f) * dt;

        if (Owner.Velocity.Y > MaxFallSpeed)
            Owner.Velocity.Y = MaxFallSpeed;
    }

    private static float MoveToward(float current, float target, float maxDelta)
    {
        if (Math.Abs(target - current) <= maxDelta)
            return target;

        return current + Math.Sign(target - current) * maxDelta;
    }
}