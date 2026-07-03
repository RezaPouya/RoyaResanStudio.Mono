using RoyaResan.Mono2d.Physics;

namespace RoyaResan.Mono2d.AI;

/// <summary>
/// Shared awareness + coordination hub that multiple enemies register
/// with. Two things this buys you that per-enemy FSMs alone can't:
///
/// 1. Group alert - one enemy spotting the target alerts the whole group,
///    instead of each enemy needing its own line of sight independently.
/// 2. Attack slots - only MaxSimultaneousAttackers enemies can be
///    "attacking" at once; the rest wait their turn instead of all
///    dogpiling the player simultaneously. This is what makes a group of
///    enemies feel coordinated rather than just independently aggressive.
/// </summary>
public class CombatGroup
{
    public readonly List<EnemyFsm> Members = new();

    public int MaxSimultaneousAttackers = 1;
    private readonly HashSet<EnemyFsm> _activeAttackers = new();

    /// <summary>Usually the player.</summary>
    public PhysicsBody Target;

    public bool AlertedToTarget;

    public void Join(EnemyFsm member) => Members.Add(member);

    public void Leave(EnemyFsm member)
    {
        Members.Remove(member);
        _activeAttackers.Remove(member);
    }

    /// <summary>Call when any member spots the target - alerts the whole group at once.</summary>
    public void RaiseAlert(PhysicsBody target)
    {
        Target = target;
        AlertedToTarget = true;
    }

    public void ClearAlert()
    {
        AlertedToTarget = false;
        Target = null;
    }

    /// <summary>Returns true if the member already holds a slot or successfully claimed one.</summary>
    public bool RequestAttackSlot(EnemyFsm member)
    {
        if (_activeAttackers.Contains(member))
            return true;

        if (_activeAttackers.Count >= MaxSimultaneousAttackers)
            return false;

        _activeAttackers.Add(member);
        return true;
    }

    public void ReleaseAttackSlot(EnemyFsm member) => _activeAttackers.Remove(member);
}
