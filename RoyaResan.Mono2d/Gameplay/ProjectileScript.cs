using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Core;
using RoyaResan.Mono2d.Scripting;

public class ProjectileScript : Script
{
    public Scene Scene;
    public Hitbox Hitbox;

    public float Lifetime = 3f;
    public float MaxDistance = 700f;
    public float StoppedSpeedThreshold = 2f;
    public float BounceDespawnDelay = 0.1f;

    private Vector2 _spawnPosition;
    private float _age;
    private bool _despawnQueued;
    private float _despawnTimer;

    public override void Start()
    {
        _spawnPosition = Owner.Position;
        Owner.IsProjectile = true;
        Hitbox.BeginSwing();
        Hitbox.Active = true;

        Scene.Combat.OnHit += OnHit;
        Scene.Combat.OnBlocked += OnBlocked;
    }

    private void OnHit(Hitbox hitbox, Hurtbox hurtbox)
    {
        if (hitbox == Hitbox) QueueDespawn(0f);
    }

    private void OnBlocked(Hitbox hitbox, Hurtbox hurtbox)
    {
        if (hitbox == Hitbox)
        {
            Owner.Velocity = new Vector2(-Owner.Velocity.X * 0.5f, Owner.Velocity.Y);
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
            if (_despawnTimer <= 0f) { Cleanup(); return; }
        }

        _age += dt;
        float traveled = Vector2.Distance(Owner.Position, _spawnPosition);

        bool stopped = Math.Abs(Owner.Velocity.X) < StoppedSpeedThreshold;
        bool grounded = Owner.IsGrounded;

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