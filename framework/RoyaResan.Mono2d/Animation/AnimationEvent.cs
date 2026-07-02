namespace RoyaResan.Mono2d.Animation;

public class AnimationEvent
{
    public int FrameIndex { get; set; }

    public Action Action { get; set; }

    internal bool HasFired { get; set; }

    public AnimationEvent(int frameIndex, Action action)
    {
        FrameIndex = frameIndex;
        Action = action;
    }

    public void Reset()
    {
        HasFired = false;
    }

    public void TryExecute(int currentFrame)
    {
        if (HasFired)
            return;

        if (currentFrame == FrameIndex)
        {
            Action?.Invoke();
            HasFired = true;
        }
    }
}