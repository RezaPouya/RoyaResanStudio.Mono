using RoyaResan.Mono2d.Animation;
using RoyaResan.Mono2d.Physics;

namespace RoyaResan.Mono2d.AI;

/// <summary>
/// Drives one enemy through named states, the same shape as your
/// Animator state machine (AddState/ChangeState) so AI logic reads the
/// same way animation logic already does.
/// </summary>
public class EnemyFsm
{
    /// <summary>The enemy's own body - set by EnemyFsmScript.Start().</summary>
    public PhysicsBody Body;

    /// <summary>Optional - if set, states can drive it directly (Machine.Animator?.Play(...)).</summary>
    public Animator Animator;

    /// <summary>Optional - shared coordination hub across multiple enemies. See CombatGroup.</summary>
    public CombatGroup Group;

    /// <summary>Optional - needed by states that raycast for line-of-sight (see VisionCone).</summary>
    public PhysicsWorld World;

    /// <summary>Optional - needed by DeadState (despawn) and RangedAttackState (spawning a projectile).</summary>
    public Core.Scene Scene;

    /// <summary>Shared facing direction, kept in sync by PatrolState/ChaseState. Read by anything that needs "which way is this enemy looking" without owning its own copy - ShieldBlockScript, projectile aim, etc.</summary>
    public Vector2 FacingDirection = Vector2.UnitX;

    private readonly Dictionary<string, EnemyState> _states = new();
    private EnemyState _current;

    public string CurrentStateName { get; private set; }

    public void AddState(string name, EnemyState state)
    {
        state.Machine = this;
        _states[name] = state;
    }

    /// <summary>
    /// Switch state. By default a no-op if already in that state - pass
    /// force: true to re-enter the same state (e.g. AttackState swinging
    /// again while the target is still in range).
    /// </summary>
    public void ChangeState(string name, bool force = false)
    {
        if (!force && name == CurrentStateName)
            return;

        if (!_states.TryGetValue(name, out var next))
            return;

        _current?.Exit();
        _current = next;
        CurrentStateName = name;
        _current.Enter();
    }

    public void Update(GameTime gameTime)
    {
        _current?.Update(gameTime);
    }
}
