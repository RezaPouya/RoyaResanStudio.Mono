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

    public virtual void Enter()
    { }

    public virtual void Update(GameTime gameTime)
    { }

    public virtual void Exit()
    { }
}