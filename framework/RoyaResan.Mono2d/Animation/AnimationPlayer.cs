using Microsoft.Xna.Framework;

namespace RoyaResan.Mono2d.Animation;

public class AnimationPlayer
{
    public AnimationClip CurrentClip { get; private set; }

    private float _timer;
    private int _frameIndex;

    public int FrameIndex => _frameIndex;
    public bool IsFinished { get; private set; }

    public Rectangle CurrentFrame
    {
        get
        {
            if (CurrentClip == null || CurrentClip.Frames.Length == 0)
                return Rectangle.Empty;

            return CurrentClip.GetFrame(_frameIndex);
        }
    }

    public void Play(AnimationClip clip, bool restart = true)
    {
        if (clip == null)
            return;

        if (!restart && CurrentClip == clip)
            return;

        CurrentClip = clip;
        _timer = 0f;
        _frameIndex = 0;
        IsFinished = false;

        ResetEvents();
    }

    // ✅ NOW CLOCK IS REQUIRED
    public void Update(AnimationClock clock)
    {
        if (CurrentClip == null || CurrentClip.Frames.Length == 0)
            return;

        if (IsFinished && !CurrentClip.IsLooping)
            return;

        _timer += clock.DeltaTime;

        while (_timer >= CurrentClip.FrameTime)
        {
            _timer -= CurrentClip.FrameTime;
            _frameIndex++;

            if (_frameIndex >= CurrentClip.Frames.Length)
            {
                if (CurrentClip.IsLooping)
                {
                    _frameIndex = 0;
                    ResetEvents();
                }
                else
                {
                    _frameIndex = CurrentClip.Frames.Length - 1;
                    IsFinished = true;
                    break;
                }
            }

            TriggerEvents();
        }
    }

    private void TriggerEvents()
    {
        if (CurrentClip?.Events == null)
            return;

        foreach (var e in CurrentClip.Events)
        {
            e.TryExecute(_frameIndex);
        }
    }

    private void ResetEvents()
    {
        if (CurrentClip?.Events == null)
            return;

        foreach (var e in CurrentClip.Events)
        {
            e.Reset();
        }
    }

    public void Reset()
    {
        _timer = 0f;
        _frameIndex = 0;
        IsFinished = false;
        ResetEvents();
    }
}