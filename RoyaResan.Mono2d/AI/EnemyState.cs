namespace RoyaResan.Mono2d.AI;

/// <summary>
/// One node in an EnemyFsm. Enter/Exit bracket a state's lifetime so
/// cleanup (stopping movement, releasing an attack slot, disarming a
/// hitbox) can't be forgotten - see AttackState for why that matters.
/// </summary>
public abstract class EnemyState
{
    /// <summary>Set automatically by EnemyFsm.AddState.</summary>
    public EnemyFsm Machine;

    private float _proximityTimer;

    public virtual void Enter()
    { }

    public virtual void Update(GameTime gameTime)
    { }

    public virtual void Exit()
    { }

    /// <summary>
    /// Backstab/blind-spot safety net for states that otherwise only
    /// notice the target through a facing-dependent VisionCone - which by
    /// design never sees anything behind the enemy. This catches the two
    /// cases that should still register regardless of facing:
    /// actually touching the target's collider (alerts immediately), or
    /// lingering within proximityRange for proximityAlertTime seconds (the
    /// enemy "feels" someone standing right behind it even without
    /// looking). Returns true the moment either condition is met; call
    /// every Update alongside the normal Vision check.
    /// </summary>
    protected bool CheckProximityAlert(PhysicsBody target, float proximityRange, float proximityAlertTime, float dt)
    {
        var body = Machine.Body;
        if (target == null || body == null)
            return false;

        if (body.Collider != null && target.Collider != null &&
            body.Collider.Bounds.Intersects(target.Collider.Bounds))
        {
            _proximityTimer = 0f;
            return true; // actual collision - always instant, no dwell time needed
        }

        float dist = Vector2.Distance(body.GlobalPosition, target.GlobalPosition);
        if (dist <= proximityRange)
        {
            _proximityTimer += dt;
            if (_proximityTimer >= proximityAlertTime)
                return true;
        }
        else
        {
            _proximityTimer = 0f; // left the range - dwell time doesn't carry over to a later approach
        }

        return false;
    }

    /// <summary>
    /// Horizontal repulsion from other members of the same CombatGroup
    /// within `range`, scaled up to `strength` at point-blank and down to
    /// 0 at `range`. This is what keeps chasing enemies from stacking on
    /// top of each other now that they don't physically collide (see
    /// PhysicsWorld.ResolveDynamicPair) - add the return value onto a
    /// state's own desired horizontal velocity, don't replace it.
    /// </summary>
    protected float ComputeSeparation(float range, float strength)
    {
        var body = Machine.Body;
        var group = Machine.Group;
        if (group == null || body == null || range <= 0f)
            return 0f;

        float total = 0f;

        foreach (var member in group.Members)
        {
            if (member == Machine || member.Body == null)
                continue;

            float d = member.Body.GlobalPosition.X - body.GlobalPosition.X;
            float ad = Math.Abs(d);

            if (ad > 0.01f && ad < range)
                total += -Math.Sign(d) * (range - ad) / range * strength;
        }

        return total;
    }
}