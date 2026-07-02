using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoyaResan.Mono2d.Animation;

public class AnimationClip
{
    public AnimationClip()
    { }

    public AnimationClip(Texture2D texture, int frameWidth, int frameHeight, float frameTime)
    {
        Texture = texture;
        FrameWidth = frameWidth;
        FrameHeight = frameHeight;
        FrameTime = frameTime;

        GenerateFrames();
    }

    public string Name { get; set; } = string.Empty;

    public Texture2D Texture { get; set; }

    public int FrameWidth { get; set; }
    public int FrameHeight { get; set; }

    public float FrameTime { get; set; } = 0.1f;

    public bool IsLooping { get; set; } = true;

    public Rectangle[] Frames { get; private set; } = Array.Empty<Rectangle>();

    public List<AnimationEvent> Events { get; set; } = new();

    public float Duration => Frames.Length * FrameTime;

    public void GenerateFrames()
    {
        if (Texture == null || FrameWidth <= 0 || FrameHeight <= 0)
            return;

        int columns = Texture.Width / FrameWidth;
        int rows = Texture.Height / FrameHeight;

        var frames = new List<Rectangle>();

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                frames.Add(new Rectangle(
                    x * FrameWidth,
                    y * FrameHeight,
                    FrameWidth,
                    FrameHeight
                ));
            }
        }

        Frames = frames.ToArray();
    }

    public Rectangle GetFrame(int index)
    {
        if (Frames == null || Frames.Length == 0)
            return Rectangle.Empty;

        if (IsLooping)
            index %= Frames.Length;
        else
            index = MathHelper.Clamp(index, 0, Frames.Length - 1);

        return Frames[index];
    }

    public bool IsValid()
    {
        return Texture != null &&
               FrameWidth > 0 &&
               FrameHeight > 0 &&
               Frames != null &&
               Frames.Length > 0;
    }
}