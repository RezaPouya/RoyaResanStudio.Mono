namespace RoyaResan.Mono2d.AI.States;

public class PatrolState : EnemyState
{
    public string AnimationState = "Run";
    public float LeftBound, RightBound;
    public float Speed = 60f;

    /// <summary>Used only as a fallback distance check when Vision isn't set.</summary>
    public float VisionRange = 150f;

    /// <summary>Optional - if set, uses full cone + line-of-sight + target visibility instead of a plain distance check. FacingDirection is kept in sync with patrol movement automatically.</summary>
    public VisionCone Vision;

    /// <summary>
    /// If true (default), the enemy turns around instead of walking off a
    /// ledge - checked by casting straight down from a point just ahead
    /// of its feet each frame. Requires Machine.World to be set (see
    /// EnemyFsm.World); if it isn't, edge avoidance is silently skipped
    /// and the enemy behaves as before (can walk off ledges).
    /// </summary>
    public bool AvoidEdges = true;

    /// <summary>How far ahead of its own edge to probe for ground, in pixels.</summary>
    public float EdgeCheckAhead = 12f;

    /// <summary>
    /// How far below its feet the ground probe reaches. Too small and it
    /// will miss real ground (false "edge"); too large and it will treat
    /// a small, safe one-step drop as a ledge to avoid.
    /// </summary>
    public float EdgeCheckDepth = 20f;

    private int _direction = 1;

    public override void Enter() => Machine.Animator?.Play(AnimationState, 0.1f);

    public override void Update(GameTime gameTime)
    {
        var body = Machine.Body;

        if (body.GlobalPosition.X <= LeftBound) _direction = 1;
        if (body.GlobalPosition.X >= RightBound) _direction = -1;

        if (AvoidEdges && Machine.World != null && !GroundAheadOf(body, _direction))
            _direction *= -1;

        body.Velocity = new Vector2(_direction * Speed, body.Velocity.Y);

        if (Vision != null)
            Vision.FacingDirection = new Vector2(_direction, 0);

        Machine.FacingDirection = new Vector2(_direction, 0);

        var group = Machine.Group;
        if (group?.Target == null)
            return;

        if (group.AlertedToTarget)
        {
            Machine.ChangeState("Chase");
            return;
        }

        bool spotted = Vision != null && Machine.World != null
            ? Vision.CanSee(Machine.World, Machine.Body, group.Target)
            : Vector2.Distance(body.GlobalPosition, group.Target.GlobalPosition) <= VisionRange;

        if (spotted)
        {
            group.RaiseAlert(group.Target);
            Machine.ChangeState("Chase");
        }
    }

    /// <summary>
    /// Casts straight down from a point just past the enemy's leading edge
    /// (in the direction it's currently walking) to see if there's ground
    /// there. Only static geometry counts as "ground" here, matching how
    /// PhysicsWorld itself only sweeps dynamic bodies against static
    /// colliders - this also naturally covers moving platforms, since
    /// they're marked IsStatic too.
    /// </summary>
    private bool GroundAheadOf(PhysicsBody body, int direction)
    {
        float halfWidth = (body.Collider?.Size.X ?? 32f) / 2f;
        float halfHeight = (body.Collider?.Size.Y ?? 32f) / 2f;

        Vector2 origin = new Vector2(
            body.GlobalPosition.X + direction * (halfWidth + EdgeCheckAhead),
            body.GlobalPosition.Y + halfHeight - 2f); // just above the feet, so the probe starts outside ground already stood on

        return Raycast.Cast(Machine.World, origin, Vector2.UnitY, EdgeCheckDepth, out _, b => b.IsStatic, ignore: body);
    }

    public override void Exit()
    {
        Machine.Body.Velocity = new Vector2(0, Machine.Body.Velocity.Y);
    }
}