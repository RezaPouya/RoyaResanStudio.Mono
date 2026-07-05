using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Core;
using RoyaResan.Mono2d.Scripting;

namespace RoyaResan.Mono2d.Gameplay;

/// <summary>
/// Generic straight-line projectile: flies at a fixed velocity, carries
/// a Hitbox for damage, and despawns itself when any of these happen:
///   - it connects with a hurtbox (Hitbox.HasHitAnyTarget)
///   - it travels past MaxDistance from where it was spawned
///   - Lifetime seconds pass
///   - it gets stopped dead by a solid Collider (a wall) - detected as
///     "was moving horizontally, now isn't," reusing PhysicsWorld's own
///     collision response instead of a separate raycast
///
/// Shared by both the player's kunai and the ranged enemy's shot - build
/// once, reuse for any "spawn a small moving hitbox that goes away when
/// it hits something" need.
///
/// Usage (see KunaiThrowScript for the full spawn pattern):
///   var body = new PhysicsBody { UseGravity = false };
///   body.Collider = new Collider { Owner = body, Size = new Vector2(10, 6) };
///   var hitbox = new Hitbox { Owner = body, Damage = 1, Size = new Vector2(10, 6) };
///   scene.AddBody(body);
///   scene.AddHitbox(hitbox);
///   body.Velocity = new Vector2(facingRight ? 500f : -500f, 0f);
///   body.AddScript(new ProjectileScript { Scene = scene, Hitbox = hitbox });
///
/// Note on immediate despawn: this removes itself from the tree during
/// its own Update() call. Node.Update iterates children by index, so
/// self-removal is safe (won't throw) but can cause the sibling that
/// shifts into this slot to skip one Update tick that same frame -
/// harmless for a fast-moving projectile, worth remembering if you reuse
/// this pattern somewhere update-order-sensitive.
/// </summary>
public class ProjectileScript : Script
{
    public Scene Scene;
    public Hitbox Hitbox;

    public float Lifetime = 2f;
    public float MaxDistance = 600f;

    /// <summary>Below this horizontal speed (after having been launched), the projectile is considered "stopped by something solid."</summary>
    public float StoppedSpeedThreshold = 20f;

    private Vector2 _spawnPosition;
    private float _age;
    private bool _armed; // guards against the stopped-speed check firing on the very first frame before velocity is even read

    public override void Start()
    {
        _spawnPosition = Owner.Position;
        Hitbox.BeginSwing();
        Hitbox.Active = true;
    }

    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _age += dt;

        if (!_armed && _age > 0.05f)
            _armed = true;

        float traveled = Vector2.Distance(Owner.Position, _spawnPosition);
        bool stoppedByWall = _armed && Math.Abs(Owner.Velocity.X) < StoppedSpeedThreshold;

        if (Hitbox.HasHitAnyTarget || _age >= Lifetime || traveled >= MaxDistance || stoppedByWall)
        {
            Scene.RemoveHitbox(Hitbox);
            Scene.RemoveBody(Owner);
        }
    }
}