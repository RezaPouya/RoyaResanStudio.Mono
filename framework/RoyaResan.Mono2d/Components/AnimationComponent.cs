using RoyaResan.Mono2d.Animation;

namespace RoyaResan.Mono2d.Components;

public class AnimationComponent : Component
{
    public Animator Animator { get; } = new();

    public void Play(AnimationClip clip)
    {
        Animator.Play(clip);
    }

    public override void Update(float dt)
    {
        Animator.Update(new Microsoft.Xna.Framework.GameTime());
    }
}


