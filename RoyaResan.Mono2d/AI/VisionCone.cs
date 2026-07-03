using System;
using RoyaResan.Mono2d.Physics;

namespace RoyaResan.Mono2d.AI;

/// <summary>
/// Full stealth vision check: target must be within Range (scaled by the
/// target's StealthModifier if it has one), within HalfAngleDegrees of
/// FacingDirection, and have a clear line of sight (no static wall
/// Collider in the way).
/// </summary>
public class VisionCone
{
    public float Range = 200f;
    public float HalfAngleDegrees = 45f;

    /// <summary>Update this from movement/facing state - e.g. (1,0) facing right, (-1,0) facing left.</summary>
    public Vector2 FacingDirection = Vector2.UnitX;

    public bool CanSee(PhysicsWorld world, PhysicsBody self, PhysicsBody target)
    {
        if (self == null || target == null)
            return false;

        Vector2 toTarget = target.GlobalPosition - self.GlobalPosition;
        float dist = toTarget.Length();
        if (dist < 0.0001f)
            return true;

        float visibility = target.Stealth?.VisibilityMultiplier ?? 1f;
        float effectiveRange = Range * MathHelper.Clamp(visibility, 0f, 1f);

        if (dist > effectiveRange)
            return false;

        Vector2 dir = toTarget / dist;
        Vector2 facing = FacingDirection.LengthSquared() > 0.0001f ? Vector2.Normalize(FacingDirection) : Vector2.UnitX;

        float dot = Vector2.Dot(facing, dir);
        float angleCos = MathF.Cos(MathHelper.ToRadians(HalfAngleDegrees));

        if (dot < angleCos)
            return false;

        return Raycast.HasLineOfSight(world, self.GlobalPosition, target.GlobalPosition, self, target);
    }
}
