using Microsoft.Xna.Framework;

namespace RoyaResan.Mono2d.Animation;

public class Animator
{
    private readonly AnimationPlayer _player = new();
    private readonly AnimationClock _clock = new();

    public AnimationClip CurrentClip => _player.CurrentClip;
    public int FrameIndex => _player.FrameIndex;
    public bool IsFinished => _player.IsFinished;
    public Rectangle CurrentFrame => _player.CurrentFrame;

    public AnimationClock Clock => _clock;

    public void Play(AnimationClip clip, bool restart = true)
    {
        _player.Play(clip, restart);
    }

    public void Update(GameTime gameTime)
    {
        //float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _clock.Update(gameTime);
        _player.Update(_clock);
    }

    public void Reset()
    {
        _player.Reset();
        _clock.Reset();
    }
}