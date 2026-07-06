namespace RoyaResan.Mono2d.AI.States;

public class ChaseState : EnemyState
{
    public string AnimationState = "Run";
    public float Speed = 100f;
    public float AttackRange = 32f;

    /// <summary>
    /// If true (default), the enemy stops at a ledge instead of running
    /// off it mid-chase - same ground probe PatrolState uses. Unlike
    /// PatrolState it doesn't turn around (that would abandon the chase);
    /// it just holds position at the edge until the target either comes
    /// back into AttackRange or the ground reappears.
    /// </summary>
    public bool AvoidEdges = true;

    public float EdgeCheckAhead = 12f;
    public float EdgeCheckDepth = 20f;

    /// <summary>Horizontal repulsion range/strength from other members of the same group - see EnemyState.ComputeSeparation. 0 disables (a single solo enemy doesn't need this).</summary>
    public float SeparationRange = 40f;
    public float SeparationStrength = 60f;

    /// <summary>If the target gets further than this for LoseSightTime seconds straight, give up and go home instead of chasing forever.</summary>
    public float LoseRange = 420f;
    public float LoseSightTime = 2f;
    private float _loseTimer;

    /// <summary>
    /// If true, this archetype throws a ranged attack instead of just
    /// standing at the edge when the target is unreachable by melee - a
    /// large enough height difference (player up on a platform, or down
    /// in a pit) switches to the "RockThrow" state instead of continuing
    /// to chase horizontally into a wall/ledge. Off by default - only the
    /// melee/shield archetypes opt in (see World.cs).
    /// </summary>
    public bool CanThrowAtRange = false;
    public float RockThrowHeightThreshold = 60f;

    public override void Enter()
    {
        Machine.Animator?.Play(AnimationState, 0.1f);
        _loseTimer = 0f;
    }

    public override void Update(GameTime gameTime)
    {
        var body = Machine.Body;
        var target = Machine.Group?.Target;

        if (target == null)
        {
            Machine.ChangeState("Idle");
            return;
        }

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        float dx = target.GlobalPosition.X - body.GlobalPosition.X;
        float dist = Math.Abs(dx);

        // Lose-sight timeout - give up and head home rather than chasing
        // forever once the target is far enough away for long enough.
        if (dist > LoseRange)
        {
            _loseTimer += dt;
            if (_loseTimer >= LoseSightTime)
            {
                Machine.ChangeState("Return");
                return;
            }
        }
        else
        {
            _loseTimer = 0f;
        }

        if (dist <= AttackRange)
        {
            Machine.ChangeState("Attack");
            return;
        }

        float heightDiff = target.GlobalPosition.Y - body.GlobalPosition.Y;

        if (CanThrowAtRange && Math.Abs(heightDiff) > RockThrowHeightThreshold)
        {
            Machine.ChangeState("RockThrow");
            return;
        }

        float dir = dx < 0 ? -1f : 1f;

        if (AvoidEdges && Machine.World != null &&
            !PatrolState.GroundAheadOf(Machine.World, body, (int)dir, EdgeCheckAhead, EdgeCheckDepth))
        {
            body.Velocity = new Vector2(0f, body.Velocity.Y); // hold at the edge instead of running off it
        }
        else
        {
            float separation = ComputeSeparation(SeparationRange, SeparationStrength);
            body.Velocity = new Vector2(dir * Speed + separation, body.Velocity.Y);
        }

        Machine.FacingDirection = new Vector2(dir, 0);
    }

    public override void Exit()
    {
        Machine.Body.Velocity = new Vector2(0, Machine.Body.Velocity.Y);
    }
}
