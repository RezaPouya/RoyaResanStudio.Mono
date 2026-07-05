namespace RoyaResan.Mono2d.AI.States;

public class IdleState : EnemyState
{
    public string AnimationState = "Idle";

    /// <summary>Used only as a fallback distance check when Vision isn't set.</summary>
    public float VisionRange = 150f;

    /// <summary>Optional - if set, uses full cone + line-of-sight + target visibility instead of a plain distance check.</summary>
    public VisionCone Vision;

    public string NextState = "Chase";

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

        if (spotted)
        {
            group.RaiseAlert(group.Target);
            Machine.ChangeState(NextState);
        }
    }
}