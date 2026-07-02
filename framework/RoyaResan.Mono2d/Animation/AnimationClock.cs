using Microsoft.Xna.Framework;

namespace RoyaResan.Mono2d.Animation;

public class AnimationClock
{
    public float TimeScale { get; set; } = 1f;
    public bool IsPaused { get; set; }

    public float DeltaTime { get; private set; }
    public float TotalTime { get; private set; }

    public void Update(GameTime gameTime)
    {
        if (IsPaused)
        {
            DeltaTime = 0f;
            return;
        }

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        DeltaTime = dt * TimeScale;
        TotalTime += DeltaTime;
    }

    public void Reset()
    {
        DeltaTime = 0f;
        TotalTime = 0f;
    }
}