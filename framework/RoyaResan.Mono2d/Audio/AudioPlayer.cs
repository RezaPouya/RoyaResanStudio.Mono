using Microsoft.Xna.Framework;

namespace RoyaResan.Mono2d.Audio;

public class AudioPlayer
{
    public void Play(AudioClip clip, float volume = 1f, float pitch = 0f, float pan = 0f)
    {
        if (clip?.Sound == null)
            return;

        var instance = clip.Sound.CreateInstance();

        instance.Volume = MathHelper.Clamp(volume * clip.Volume, 0f, 1f);
        instance.Pitch = pitch;
        instance.Pan = pan;

        instance.Play();
    }
}