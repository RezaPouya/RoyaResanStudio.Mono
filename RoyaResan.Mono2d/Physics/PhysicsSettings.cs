namespace RoyaResan.Mono2d.Physics;

/// <summary>
/// Shared gravity constant. Off per-body by default (PhysicsBody.UseGravity
/// = false) so existing top-down-style movement (e.g. PlayerMovementScript)
/// isn't silently affected - opt a body in explicitly for platforming or
/// rope-swing use.
/// </summary>
public static class PhysicsSettings
{
    /// <summary>Pixels/sec^2. Positive = downward (Y+ is down in this framework).</summary>
    public static float Gravity = 900f;
}