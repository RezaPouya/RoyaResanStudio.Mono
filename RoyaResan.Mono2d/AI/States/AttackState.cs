using RoyaResan.Mono2d.Combat;

namespace RoyaResan.Mono2d.AI.States;

/// <summary>
/// If Machine.Group is set, this state must claim an attack slot before
/// it can actually swing - if the group is already at
/// MaxSimultaneousAttackers, this enemy stands ready instead of piling
/// on, and keeps retrying each frame until a slot frees up. That's the
/// entire "enemies work together" behavior: it falls out of this one
/// check, not a scripted choreography.
/// </summary>
public class AttackState : EnemyState
{
    public string AnimationState = "Attack";
    public float AttackDuration = 0.5f;

    /// <summary>Hitbox is only Active between these two points in the swing (seconds).</summary>
    public float ActiveWindowStart = 0.15f;
    public float ActiveWindowEnd = 0.3f;

    public float AttackRange = 32f;

    /// <summary>Optional - if set, armed automatically during the active window.</summary>
    public Hitbox Hitbox;

    private float _timer;
    private bool _gotSlot;

    public override void Enter()
    {
        _timer = 0f;
        _gotSlot = Machine.Group == null || Machine.Group.RequestAttackSlot(Machine);

        Machine.Animator?.Play(_gotSlot ? AnimationState : "Idle", 0.05f);

        if (_gotSlot)
            Hitbox?.BeginSwing();
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

        if (!_gotSlot)
        {
            _gotSlot = Machine.Group.RequestAttackSlot(Machine);
            if (_gotSlot)
            {
                Machine.Animator?.Play(AnimationState, 0.05f);
                Hitbox?.BeginSwing();
            }
            return; // still waiting - don't advance the swing timer
        }

        _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (Hitbox != null)
            Hitbox.Active = _timer >= ActiveWindowStart && _timer <= ActiveWindowEnd;

        if (_timer >= AttackDuration)
        {
            if (Hitbox != null)
                Hitbox.Active = false;

            Machine.Group?.ReleaseAttackSlot(Machine);

            float dist = Vector2.Distance(body.GlobalPosition, target.GlobalPosition);
            if (dist > AttackRange * 1.5f)
                Machine.ChangeState("Chase");
            else
                Machine.ChangeState("Attack", force: true); // still in range - swing again
        }
    }

    public override void Exit()
    {
        if (Hitbox != null)
            Hitbox.Active = false;

        Machine.Group?.ReleaseAttackSlot(Machine);
    }
}
