using RoyaResan.Mono2d.Audio;

namespace RoyaResan.Mono2d.Components;

public class AudioComponent : Component
{
    private readonly AudioSystem _audio;

    public AudioComponent(AudioSystem audio)
    {
        _audio = audio;
    }

    public void Play(AudioClip clip)
    {
        _audio.Play(clip);
    }
}


