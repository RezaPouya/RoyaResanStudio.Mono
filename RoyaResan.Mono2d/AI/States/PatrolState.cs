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

    /// <summary>Distance at which "lingering nearby" starts counting, regardless of facing - see EnemyState.CheckProximityAlert.</summary>
    public float ProximityRange = 48f;

    /// <summary>Seconds the target must stay within ProximityRange before this enemy notices it without needing line of sight.</summary>
    public float ProximityAlertTime = 1.2f;

    /// <summary>
    /// If true (default), the enemy turns around instead of walking off a
    /// ledge - checked each frame by testing for solid ground just past
    /// its leading edge. Requires Machine.World to be set (see
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

        if (AvoidEdges && Machine.World != null && !GroundAheadOf(Machine.World, body, _direction, EdgeCheckAhead, EdgeCheckDepth))
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

        if (!spotted)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            spotted = CheckProximityAlert(group.Target, ProximityRange, ProximityAlertTime, dt);
        }

        if (spotted)
        {
            group.RaiseAlert(group.Target);
            Machine.ChangeState("Chase");
        }
    }

    /// <summary>
    /// Direct AABB probe for solid ground just past the enemy's leading
    /// edge, in the direction it's currently moving - internal, static so
    /// ChaseState can reuse the exact same check. Deliberately NOT a
    /// raycast: it mirrors the xOverlaps/yOverlaps pattern PhysicsWorld's
    /// own sweep already uses, which sidesteps a subtle float/int
    /// truncation edge case in the general-purpose Raycast helper's
    /// point-in-rect test that could misfire exactly at the pixel a body
    /// is resting on - the case that matters most here.
    /// </summary>
    internal static bool GroundAheadOf(PhysicsWorld world, PhysicsBody body, int direction, float ahead, float depth)
    {
        float halfWidth = (body.Collider?.Size.X ?? 32f) / 2f;
        float halfHeight = (body.Collider?.Size.Y ?? 32f) / 2f;

        float probeX = body.GlobalPosition.X + direction * (halfWidth + ahead);
        float feetY = body.GlobalPosition.Y + halfHeight;
        float probeBottom = feetY + depth;

        foreach (var other in world.Bodies)
        {
            if (other == body || other.Collider == null || !other.IsStatic)
                continue; // only static geometry counts as "ground" - matches PhysicsWorld's own sweep, and naturally covers moving platforms too since they're marked IsStatic

            var bounds = other.Collider.Bounds;

            bool xOverlaps = probeX > bounds.Left && probeX < bounds.Right;
            bool yOverlaps = feetY <= bounds.Bottom && probeBottom >= bounds.Top;

            if (xOverlaps && yOverlaps)
                return true;
        }

        return false;
    }

    public override void Exit()
    {
        Machine.Body.Velocity = new Vector2(0, Machine.Body.Velocity.Y);
    }
}
