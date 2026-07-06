using RoyaResan.Mono2d.Combat;
using RoyaResan.Mono2d.Gameplay;

namespace RoyaResan.Mono2d.AI.States;

public class RangedAttackState : EnemyState
{
    public string AnimationState = "Attack";
    public float FireInterval = 1.5f;
    public float ProjectileSpeed = 380f;
    public int Damage = 1;

    public float PreferredDistance = 180f;
    public float MoveSpeed = 85f;

    public bool ProjectileUseGravity = false;
    public bool AvoidEdges = true;

    public VisionCone Vision;

    private float _timer;

    public override void Enter()
    {
        _timer = FireInterval / 2f;
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

        Vector2 toTarget = target.GlobalPosition - Machine.Body.GlobalPosition;
        float dist = toTarget.Length();
        Vector2 aimDir = dist > 0.01f ? Vector2.Normalize(toTarget) : new Vector2(1f, 0f);

        Machine.FacingDirection = new Vector2(Math.Sign(aimDir.X), 0);
        if (Vision != null)
            Vision.FacingDirection = Machine.FacingDirection;

        // Keep distance with strong edge avoidance
        float moveX = 0f;
        if (dist < PreferredDistance && Math.Abs(toTarget.X) > 20f)
            moveX = -Math.Sign(toTarget.X) * MoveSpeed;

        if (AvoidEdges && Machine.World != null)
        {
            int dir = Math.Sign(moveX);
            if (dir != 0 && !PatrolState.GroundAheadOf(Machine.World, Machine.Body, dir, 25f, 30f))
                moveX = 0f;  // Stop instead of falling
        }

        Machine.Body.Velocity = new Vector2(moveX, Machine.Body.Velocity.Y);

        _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_timer >= FireInterval)
        {
            _timer = 0f;
            Fire(aimDir);
        }
    }

    /// <summary>
    /// Zero horizontal velocity on leaving this state - matches
    /// PatrolState.Exit(). Without this, an enemy that loses sight of the
    /// target mid-retreat (Update above calls ChangeState("Idle")) carries
    /// its retreat velocity straight into Idle, which never checks edges
    /// or touches Velocity, and just keeps coasting until it walks off
    /// whatever ledge it was carefully avoiding a moment ago.
    /// </summary>
    public override void Exit()
    {
        Machine.Body.Velocity = new Vector2(0, Machine.Body.Velocity.Y);
    }

    private void Fire(Vector2 aimDir)
    { /* same as previous version */
        if (Machine.Scene == null) return;
        var body = Machine.Body;
        var shot = new PhysicsBody { UseGravity = ProjectileUseGravity, Team = "Enemy" };
        shot.Position = body.Position + aimDir * 25f;
        shot.Collider = new Collider { Owner = shot, Size = new Vector2(12f, 8f) };

        var visual = new Nodes.PlaceholderRectNode { Size = new Vector2(12f, 8f), Color = Color.OrangeRed };
        shot.AddChild(visual);

        var hitbox = new Hitbox { Owner = shot, Damage = Damage, Size = new Vector2(12f, 8f), Tag = "EnemyProjectile" };
        shot.UserData = body;

        Machine.Scene.AddBody(shot);
        Machine.Scene.AddHitbox(hitbox);

        shot.Velocity = aimDir * ProjectileSpeed;
        shot.AddScript(new ProjectileScript { Scene = Machine.Scene, Hitbox = hitbox, Lifetime = 3f });
    }
}