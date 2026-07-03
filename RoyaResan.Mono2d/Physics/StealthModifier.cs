namespace RoyaResan.Mono2d.Physics;

/// <summary>
/// Optional detectability modifier. Attach to the player (or anything
/// that should be harder to spot) and adjust VisibilityMultiplier at
/// runtime - e.g. 0.5 while crouching, lower still while standing in a
/// shadow trigger volume. VisionCone reads this to shrink its effective
/// detection range. 1 = fully visible (default with no modifier attached).
/// </summary>
public class StealthModifier
{
    public float VisibilityMultiplier = 1f;
}
