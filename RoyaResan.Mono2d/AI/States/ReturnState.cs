namespace RoyaResan.Mono2d.AI.States;

/// <summary>
/// Entered when ChaseState gives up (target too far for too long). Pauses
/// briefly - "looking around" at the last-seen spot - then walks back to
/// Machine.HomePosition (set once at spawn) and resumes Patrol, instead of
/// instantly snapping back into patrol wherever the chase happened to end.
/// Reuses PatrolState's ground probe so it doesn't walk itself off a ledge
/// on the way home either.
/// </summary>
public class ReturnState : EnemyState
{
    public string AnimationState = "Run";
    public float Speed = 60f;
    public float ArriveThreshold = 8f;

    /// <summary>Seconds spent standing still at the last-seen spot before walking home - the "did I really lose them?" pause.</summary>
    public float WaitBeforeReturn = 2f;

    public bool AvoidEdges = true;
    public float EdgeCheckAhead = 12f;
    public float EdgeCheckDepth = 20f;

    /// <summary>If the target comes back within this range while returning/waiting, drop everything and re-chase immediately.</summary>
    public float ReengageRange = 200f;

    private float _waitTimer;
    private bool _waited;

    public override void Enter()
    {
        Machine.Animator?.Play("Idle", 0.1f);
        _waitTimer = 0f;
        _waited = false;
    }

    public override void Update(GameTime gameTime)
    {
        var body = Machine.Body;
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        var target = Machine.Group?.Target;
        if (target != null)
        {
            float distToTarget = Vector2.Distance(body.GlobalPosition, target.GlobalPosition);
            if (distToTarget <= ReengageRange)
            {
                Machine.ChangeState("Chase");
                return;
            }
        }

        if (!_waited)
        {
            body.Velocity = new Vector2(0, body.Velocity.Y);
            _waitTimer += dt;
            if (_waitTimer >= WaitBeforeReturn)
                _waited = true;
            else
                return;

            Machine.Animator?.Play(AnimationState, 0.1f);
        }

        float dx = Machine.HomePosition.X - body.GlobalPosition.X;
        float dist = Math.Abs(dx);

        if (dist <= ArriveThreshold)
        {
            body.Velocity = new Vector2(0, body.Velocity.Y);
            Machine.ChangeState("Patrol");
            return;
        }

        float dir = dx < 0 ? -1f : 1f;

        if (AvoidEdges && Machine.World != null &&
            !PatrolState.GroundAheadOf(Machine.World, body, (int)dir, EdgeCheckAhead, EdgeCheckDepth))
            body.Velocity = new Vector2(0, body.Velocity.Y);
        else
            body.Velocity = new Vector2(dir * Speed, body.Velocity.Y);

        Machine.FacingDirection = new Vector2(dir, 0);
    }

    public override void Exit()
    {
        Machine.Body.Velocity = new Vector2(0, Machine.Body.Velocity.Y);
    }
}
