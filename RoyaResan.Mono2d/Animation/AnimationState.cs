namespace RoyaResan.Mono2d.Animation;

/// <summary>
/// One node in an Animator's state machine: a name, the clip it plays,
/// and that state's own base speed (combines with Animator.SpeedMultiplier
/// for environment/situation effects).
/// </summary>
public class AnimationState
{
    public string Name;
    public AnimationClip Clip;
    public float Speed = 1f;
}