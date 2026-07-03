namespace RoyaResan.Mono2d.Animation;

public class AnimationClip
{
    public string Name;

    public List<Texture2D> Frames = new();

    public float FrameTime = 0.1f; // seconds per frame

    public bool Loop = true;
}