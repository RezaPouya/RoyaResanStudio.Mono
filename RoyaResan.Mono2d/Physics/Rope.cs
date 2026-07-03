namespace RoyaResan.Mono2d.Physics;

/// <summary>
/// A simple circular distance-constraint rope (the standard technique
/// behind most 2D "grapple/swing" mechanics - Worms' ninja rope, Bionic
/// Commando-style hooks - without needing a full physics constraint
/// solver):
///
/// Each step, if Body has moved further than Length from Anchor, its
/// position is clamped back onto the circle of radius Length, and the
/// velocity component pointing further outward (radial) is removed while
/// the component along the circle (tangential) is kept. Under gravity,
/// that tangential velocity is what turns into a swinging arc instead of
/// the body just stopping dead at full extension.
///
/// Position math is done in world space (GlobalPosition) then written
/// back as a Position delta - same simplifying assumption the existing
/// collision Resolve() already makes, i.e. bodies with a rope attached
/// should sit directly under the scene root with no nested parent
/// transform.
/// </summary>
public class Rope
{
    public PhysicsBody Body;
    public Vector2 Anchor;
    public float Length;
    public bool Attached;

    public const float MinLength = 16f;

    /// <summary>Attaches at the given world point. If length &lt;= 0, uses the current distance to Body as the starting length.</summary>
    public void Attach(Vector2 anchor, float length = -1f)
    {
        Anchor = anchor;
        Length = length > 0f
            ? length
            : (Body != null ? Vector2.Distance(Body.GlobalPosition, anchor) : MinLength);
        Attached = true;
    }

    public void Detach() => Attached = false;

    /// <summary>Positive shortens (reel in), negative lengthens (reel out).</summary>
    public void Reel(float amount) => Length = System.Math.Max(MinLength, Length - amount);

    public void Step()
    {
        if (!Attached || Body == null)
            return;

        Vector2 toBody = Body.GlobalPosition - Anchor;
        float dist = toBody.Length();

        if (dist <= Length || dist < 0.0001f)
            return;

        Vector2 dir = toBody / dist;

        // Clamp position onto the circle.
        Vector2 corrected = Anchor + dir * Length;
        Body.Position += corrected - Body.GlobalPosition;

        // Strip outward (radial) velocity, keep tangential - this is what
        // produces a swing instead of a hard stop at full extension.
        float radialSpeed = Vector2.Dot(Body.Velocity, dir);
        if (radialSpeed > 0f)
            Body.Velocity -= dir * radialSpeed;
    }
}
