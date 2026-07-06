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

    private Vector2 _spawnPosition;
    private float _age;
    private bool _armed;

    public override void Start()
    {
        _spawnPosition = Owner.Position;
        Owner.IsProjectile = true; // Prevent being pushed by player/enemies
        Hitbox.BeginSwing();
        Hitbox.Active = true;
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _age += dt;

        if (!_armed && _age > 0.08f) // Slightly delayed armed to avoid spawn collision
            _armed = true;

        float traveled = Vector2.Distance(Owner.Position, _spawnPosition);
        bool stoppedByWall = _armed && Math.Abs(Owner.Velocity.X) < StoppedSpeedThreshold;
        bool landedOnGround = _armed && Owner.IsGrounded;

        if (Hitbox.HasHitAnyTarget || _age >= Lifetime || traveled >= MaxDistance || stoppedByWall || landedOnGround)
        {
            Scene.RemoveHitbox(Hitbox);
            Scene.RemoveBody(Owner);
        }
    }
}