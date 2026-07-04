using RoyaResan.Mono2d.Physics;
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
    public float MoveSpeed = 220f;
    public float Acceleration = 1600f;
    public float Deceleration = 2000f;

    /// <summary>Initial upward speed applied on jump (positive value; applied to Velocity.Y as negative).</summary>
    public float JumpVelocity = 420f;

    public float CoyoteTime = 0.1f;
    public float JumpBufferTime = 0.12f;

    /// <summary>Extra gravity multiplier while rising and jump is NOT held - lets a tapped jump cut short instead of always reaching full height.</summary>
    public float LowJumpGravityMultiplier = 2.5f;

    /// <summary>Extra gravity multiplier while falling - snappier descent than a symmetric arc.</summary>
    public float FallGravityMultiplier = 1.6f;

    public Keys JumpKey = Keys.Space;
    public Keys LeftKey = Keys.A;
    public Keys RightKey = Keys.D;

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
        if (Input.IsKeyDown(LeftKey)) inputX -= 1f;
        if (Input.IsKeyDown(RightKey)) inputX += 1f;

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
        if (Input.IsKeyPressed(JumpKey))
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
        if (_isJumpRising && Input.IsKeyReleased(JumpKey) && Owner.Velocity.Y < 0f)
            Owner.Velocity.Y *= 0.5f;

        if (Owner.Velocity.Y >= 0f)
            _isJumpRising = false; // apex passed or landed - no longer an active "held jump"

        // Extra gravity on top of PhysicsBody's base gravity integration
        // (which runs after scripts, later this same Update call) - this
        // is what actually shapes the fast-fall / short-hop arc.
        if (Owner.Velocity.Y > 0f)
            Owner.Velocity.Y += PhysicsSettings.Gravity * (FallGravityMultiplier - 1f) * dt;
        else if (Owner.Velocity.Y < 0f && !Input.IsKeyDown(JumpKey))
            Owner.Velocity.Y += PhysicsSettings.Gravity * (LowJumpGravityMultiplier - 1f) * dt;
    }

    private static float MoveToward(float current, float target, float maxDelta)
    {
        if (Math.Abs(target - current) <= maxDelta)
            return target;

        return current + Math.Sign(target - current) * maxDelta;
    }
}
