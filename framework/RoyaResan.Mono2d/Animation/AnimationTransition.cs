using System;

namespace RoyaResan.Mono2d.Animation;

/// <summary>
/// A rule the Animator checks every frame: if Condition is true (and the
/// current state matches From), switch to To, crossfading over BlendDuration.
/// </summary>
public class AnimationTransition
{
    /// <summary>Null or "*" means "from any state".</summary>
    public string From;
    public string To;
    public Func<bool> Condition;
    public float BlendDuration = 0.1f;
}
