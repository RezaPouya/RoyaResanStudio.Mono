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

    private int _direction = 1;

    public override void Enter() => Machine.Animator?.Play(AnimationState, 0.1f);

    public override void Update(GameTime gameTime)
    {
        var body = Machine.Body;

        if (body.GlobalPosition.X <= LeftBound) _direction = 1;
        if (body.GlobalPosition.X >= RightBound) _direction = -1;

        body.Velocity = new Vector2(_direction * Speed, body.Velocity.Y);

        if (Vision != null)
            Vision.FacingDirection = new Vector2(_direction, 0);

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

    public override void Exit()
    {
        Machine.Body.Velocity = new Vector2(0, Machine.Body.Velocity.Y);
    }
}
