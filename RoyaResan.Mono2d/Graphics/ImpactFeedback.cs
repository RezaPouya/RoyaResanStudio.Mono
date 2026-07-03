namespace RoyaResan.Mono2d.Graphics;

/// <summary>
/// "Juice" moments (hits, parries, landings) almost always want a camera
/// shake and a sound together. Call this instead of triggering each
/// system separately so gameplay code has one line, not two.
/// </summary>
public static class ImpactFeedback
{
    public static void Trigger(
        Camera2D camera,
        SoundEffect sound = null,
        float shakeDuration = 0.15f,
        float shakeMagnitude = 6f,
        float volume = 1f)
    {
        camera?.Shake(shakeDuration, shakeMagnitude);

        if (sound != null)
            AudioManager.PlaySfx(sound, volume);
    }
}
