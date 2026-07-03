using RoyaResan.Mono2d.Physics;

namespace RoyaResan.Mono2d.AI;

/// <summary>
/// A noise event at a world position alerts any CombatGroup member within
/// Radius, regardless of vision - sound travels around corners more than
/// sight does, so this is deliberately a plain radius check rather than
/// a raycast per listener. Once any member is in range, RaiseAlert already
/// alerts the whole group (shared state), so one hearer is enough.
/// </summary>
public static class NoiseSystem
{
    public static void Emit(CombatGroup group, Vector2 position, float radius, PhysicsBody target)
    {
        if (group == null)
            return;

        foreach (var member in group.Members)
        {
            if (member.Body == null)
                continue;

            float dist = Vector2.Distance(member.Body.GlobalPosition, position);
            if (dist <= radius)
            {
                group.RaiseAlert(target);
                return;
            }
        }
    }

    /// <summary>Convenience for a level with multiple independent enemy groups.</summary>
    public static void Emit(IEnumerable<CombatGroup> groups, Vector2 position, float radius, PhysicsBody target)
    {
        foreach (var group in groups)
            Emit(group, position, radius, target);
    }
}
