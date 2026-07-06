namespace RoyaResan.Mono2d.AI.States;

public class IdleState : EnemyState
{
    public string AnimationState = "Idle";

    /// <summary>Used only as a fallback distance check when Vision isn't set.</summary>
    public float VisionRange = 150f;

    /// <summary>Optional - if set, uses full cone + line-of-sight + target visibility instead of a plain distance check.</summary>
    public VisionCone Vision;

    public string NextState = "Chase";

    /// <summary>Distance at which "lingering nearby" starts counting, regardless of facing - see EnemyState.CheckProximityAlert.</summary>
    public float ProximityRange = 48f;

    /// <summary>Seconds the target must stay within ProximityRange before this enemy notices it without needing line of sight.</summary>
    public float ProximityAlertTime = 1.2f;

    public override void Enter() => Machine.Animator?.Play(AnimationState, 0.1f);

    public override void Update(GameTime gameTime)
    {
        var group = Machine.Group;
        if (group?.Target == null)
            return;

        if (group.AlertedToTarget)
        {
            Machine.ChangeState(NextState);
            return;
        }

        bool spotted = Vision != null && Machine.World != null
            ? Vision.CanSee(Machine.World, Machine.Body, group.Target)
            : Vector2.Distance(Machine.Body.GlobalPosition, group.Target.GlobalPosition) <= VisionRange;

        if (!spotted)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            spotted = CheckProximityAlert(group.Target, ProximityRange, ProximityAlertTime, dt);
        }

        if (spotted)
        {
            group.RaiseAlert(group.Target);
            Machine.ChangeState(NextState);
        }
    }
}