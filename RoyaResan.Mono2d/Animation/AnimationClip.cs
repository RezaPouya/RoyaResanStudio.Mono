namespace RoyaResan.Mono2d.Animation;

/// <summary>
/// A named animation made of sub-rectangles into a single spritesheet
/// texture. No per-frame texture loading - one Texture2D is shared by
/// every frame in every clip that uses the same sheet.
/// </summary>
public class AnimationClip
{
    public string Name;

    /// <summary>The one shared spritesheet texture this clip's frames come from.</summary>
    public Texture2D SpriteSheet;

    /// <summary>Sub-rectangles into SpriteSheet, one per frame, in playback order.</summary>
    public List<Rectangle> Frames = new();

    /// <summary>Seconds per frame at Speed = 1.</summary>
    public float FrameTime = 0.1f;

    public bool Loop = true;

    /// <summary>
    /// Optional: frame index -> sound to play the instant playback lands
    /// on that frame (footsteps, attack whoosh, parry clink, etc). Fires
    /// automatically from AnimationPlayer - no extra gameplay code needed.
    /// </summary>
    public Dictionary<int, SoundEffect> FrameSounds = new();

    /// <summary>
    /// Convenience builder for a clip whose frames sit in a uniform grid
    /// on the spritesheet (the common case for a sprite-sheet character).
    /// </summary>
    public static AnimationClip FromGrid(
        string name, Texture2D sheet,
        int frameWidth, int frameHeight,
        int frameCount, int columns,
        int startX = 0, int startY = 0,
        float frameTime = 0.1f, bool loop = true)
    {
        var clip = new AnimationClip
        {
            Name = name,
            SpriteSheet = sheet,
            FrameTime = frameTime,
            Loop = loop
        };

        for (int i = 0; i < frameCount; i++)
        {
            int col = i % columns;
            int row = i / columns;

            clip.Frames.Add(new Rectangle(
                startX + col * frameWidth,
                startY + row * frameHeight,
                frameWidth, frameHeight));
        }

        return clip;
    }
}