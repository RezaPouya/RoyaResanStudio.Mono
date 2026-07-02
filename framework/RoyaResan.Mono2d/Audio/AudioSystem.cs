namespace RoyaResan.Mono2d.Audio;

public class AudioSystem
{
    private readonly AudioPlayer _player = new();

    public float MasterVolume { get; set; } = 1f;

    public bool IsMuted { get; set; } = false;

    public void Play(AudioClip clip, float volume = 1f)
    {
        if (IsMuted || clip == null)
            return;

        _player.Play(clip, volume * MasterVolume);
    }
}