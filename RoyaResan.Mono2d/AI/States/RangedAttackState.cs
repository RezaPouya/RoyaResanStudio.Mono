using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Gameplay;
using RoyaResan.Mono2d.Physics;

namespace RoyaResan.Mono2d.AI.States;

/// <summary>
/// Stands in place and fires a straight-line shot at the target every
/// FireInterval seconds (reusing the same ProjectileScript the player's
/// kunai uses - one generic "spawn a moving hitbox" primitive for both).
/// Reverts to Idle the instant vision is lost, per the design doc's
/// "stops shooting if player leaves vision" - checked every frame, not
/// just at entry, so it can't keep shooting through a wall the moment
/// the target ducks behind one.
///
/// Deliberately does NOT chase - the ranged enemy archetype "remains at
/// distance" by design, so its own FSM simply never has a Chase state at
/// all (Idle -> RangedAttack -> Idle).
/// </summary>
public class RangedAttackState : EnemyState
{
    public string AnimationState = "Attack";
    public float FireInterval = 2f;
    public float ProjectileSpeed = 350f;
    public int Damage = 1;

    /// <summary>Optional - if set, re-checks line of sight every frame instead of just staying alerted forever once spotted.</summary>
    public VisionCone Vision;

    private float _timer;

    public override void Enter()
    {
        _timer = FireInterval; // fire almost immediately on entering, not after a full wait
        Machine.Animator?.Play(AnimationState, 0.1f);
    }

    public override void Update(GameTime gameTime)
    {
        var target = Machine.Group?.Target;
        if (target == null)
        {
            Machine.ChangeState("Idle");
            return;
        }

        bool canSee = Vision == null || Machine.World == null || Vision.CanSee(Machine.World, Machine.Body, target);
        if (!canSee)
        {
            Machine.ChangeState("Idle");
            return;
        }

        float dir = target.GlobalPosition.X >= Machine.Body.GlobalPosition.X ? 1f : -1f;
        Machine.FacingDirection = new Vector2(dir, 0);
        if (Vision != null)
            Vision.FacingDirection = new Vector2(dir, 0);

        _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_timer >= FireInterval)
        {
            _timer = 0f;
            Fire(dir);
        }
    }

    private void Fire(float dir)
    {
        if (Machine.Scene == null)
            return;

        var body = Machine.Body;

        var shot = new PhysicsBody { UseGravity = false };
        shot.Position = body.Position + new Vector2(dir * 20f, -6f);
        shot.Collider = new Collider { Owner = shot, Size = new Vector2(10f, 6f) };

        var visual = new Nodes.PlaceholderRectNode { Size = new Vector2(10f, 6f), Color = Color.OrangeRed };
        shot.AddChild(visual);

        var hitbox = new Hitbox { Owner = shot, Damage = Damage, Size = new Vector2(10f, 6f), Tag = "EnemyProjectile" };

        Machine.Scene.AddBody(shot);
        Machine.Scene.AddHitbox(hitbox);

        shot.Velocity = new Vector2(dir * ProjectileSpeed, 0f);
        shot.AddScript(new ProjectileScript { Scene = Machine.Scene, Hitbox = hitbox });
    }
}
