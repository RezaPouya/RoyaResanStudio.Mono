using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Core;
using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

public class ProjectileScript : Script
{
    public Scene Scene;
    public Hitbox Hitbox;

    public float Lifetime = 3f;
    public float MaxDistance = 700f;
    public float StoppedSpeedThreshold = 2f;

    public float BlockKnockback = 30f;           // horizontal knockback applied to the shield enemy on block
    public float BlockBounceSpeed = 0.5f;        // multiplier for the kunai's reverse speed (0.5 = half speed back)
    public float BounceDespawnDelay = 0.1f;      // visual delay before despawning after bounce

    private Vector2 _spawnPosition;
    private float _age;
    private bool _despawnQueued;
    private float _despawnTimer;

    public override void Start()
    {
        _spawnPosition = Owner.Position;
        Owner.IsProjectile = true;

        // Activate hitbox immediately – swept test covers full path
        Hitbox.BeginSwing();
        Hitbox.Active = true;

        // Subscribe to combat events
        Scene.Combat.OnHit += OnHit;
        Scene.Combat.OnBlocked += OnBlocked;
    }

    private void OnHit(Hitbox hitbox, Hurtbox hurtbox)
    {
        if (hitbox == Hitbox)
        {
            // Dealt damage – despawn immediately (no bounce)
            QueueDespawn(0f);
        }
    }

    private void OnBlocked(Hitbox hitbox, Hurtbox hurtbox)
    {
        if (hitbox == Hitbox)
        {
            // Blocked by shield – bounce back and apply knockback to the blocker
            var blocker = hurtbox.Owner;
            if (blocker != null && !blocker.IsStatic)
            {
                // Knock back the shield enemy away from the kunai's direction
                float dir = Math.Sign(Owner.Velocity.X);
                blocker.Velocity = new Vector2(dir * BlockKnockback, -50f); // slight upward pop
            }

            // Reverse the kunai's velocity (bounce back)
            Owner.Velocity = new Vector2(-Owner.Velocity.X * BlockBounceSpeed, Owner.Velocity.Y);
            QueueDespawn(BounceDespawnDelay);
        }
    }

    private void QueueDespawn(float delay)
    {
        if (_despawnQueued) return;
        _despawnQueued = true;
        _despawnTimer = delay;
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_despawnQueued)
        {
            _despawnTimer -= dt;
            if (_despawnTimer <= 0f)
            {
                Cleanup();
                return;
            }
            // Continue moving during bounce delay
        }

        _age += dt;
        float traveled = Vector2.Distance(Owner.Position, _spawnPosition);

        bool stopped = Math.Abs(Owner.Velocity.X) < StoppedSpeedThreshold;
        bool grounded = Owner.IsGrounded;

        // Fallback despawn (lifetime, max distance, stopped, grounded)
        if (!_despawnQueued && (_age >= Lifetime || traveled >= MaxDistance || stopped || grounded))
        {
            Cleanup();
        }
    }

    private void Cleanup()
    {
        Scene.Combat.OnHit -= OnHit;
        Scene.Combat.OnBlocked -= OnBlocked;
        Scene.RemoveHitbox(Hitbox);
        Scene.RemoveBody(Owner);
    }
}