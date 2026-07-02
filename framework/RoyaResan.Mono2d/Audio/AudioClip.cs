using Microsoft.Xna.Framework.Audio;

namespace RoyaResan.Mono2d.Audio;

public class AudioClip
{
    public string Name { get; set; } = string.Empty;

    public SoundEffect Sound { get; set; }

    public float Volume { get; set; } = 1f;

    public AudioClip() { }

    public AudioClip(SoundEffect sound)
    {
        Sound = sound;
    }
}