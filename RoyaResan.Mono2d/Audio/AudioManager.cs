namespace RoyaResan.Mono2d.Audio;

/// <summary>
/// Static audio system, mirroring Input's static-polling style. SFX are
/// one-shot SoundEffectInstances (so multiple can overlap and each can be
/// controlled independently); music is a single Song via MediaPlayer.
/// </summary>
public static class AudioManager
{
    public static float MasterVolume = 1f;
    public static float SfxVolume = 1f;
    public static float MusicVolume = 1f;

    private static Song _currentSong;

    /// <summary>Plays a one-shot sound. Returns the instance so callers can stop/fade it if needed.</summary>
    public static SoundEffectInstance PlaySfx(SoundEffect sfx, float volume = 1f, float pitch = 0f, float pan = 0f)
    {
        if (sfx == null)
            return null;

        var instance = sfx.CreateInstance();
        instance.Volume = MathHelper.Clamp(volume * SfxVolume * MasterVolume, 0f, 1f);
        instance.Pitch = MathHelper.Clamp(pitch, -1f, 1f);
        instance.Pan = MathHelper.Clamp(pan, -1f, 1f);
        instance.Play();
        return instance;
    }

    public static void PlayMusic(Song song, bool loop = true)
    {
        if (song == null)
            return;

        _currentSong = song;
        MediaPlayer.IsRepeating = loop;
        MediaPlayer.Volume = MathHelper.Clamp(MusicVolume * MasterVolume, 0f, 1f);
        MediaPlayer.Play(song);
    }

    public static void StopMusic() => MediaPlayer.Stop();

    public static void PauseMusic() => MediaPlayer.Pause();

    public static void ResumeMusic() => MediaPlayer.Resume();

    /// <summary>Call after changing MasterVolume/MusicVolume at runtime (e.g. from an options menu) to apply it to music immediately.</summary>
    public static void SyncMusicVolume()
    {
        if (_currentSong != null)
            MediaPlayer.Volume = MathHelper.Clamp(MusicVolume * MasterVolume, 0f, 1f);
    }
}