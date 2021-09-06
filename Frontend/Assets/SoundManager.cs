using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public static float FxVolume;
    public static float MusicVolume;

    public static void Play(SoundItem item)
    {
        if (item == null)
        {
            Debug.LogError($"Sound item is null");
            return;
        }

        if (item.clips == null || item.clips.Length == 0)
        {
            Debug.LogError($"Sound item has no audio clips, comment: {item.comment}");
            return;
        }

        bool isOnCooldown = Time.time < item.timeLastPlayed + item.cooldown;
        if (isOnCooldown)
            return;

        var clip = item.clips[Random.Range(0, item.clips.Length)];
        float pitch = item.pitch + (Random.value * item.pitchVariation * 2) - item.pitchVariation * 0.5f;
        float volume = item.volume + (Random.value * item.volumeVariation * 2) - item.volumeVariation * 0.5f;
        volume *= FxVolume;

        item.audioSource.pitch = pitch;
        item.audioSource.volume = volume;
        item.audioSource.clip = clip;
        item.audioSource.Play();
        item.timeLastPlayed = Time.time;
    }

    public static void SetFxVolume(float volume) => FxVolume = volume;
    public static void SetMusicVolume(float volume) => MusicVolume = volume;

    public static void SaveSettings()
    {
        PlayerPrefs.SetFloat("music_volume", MusicVolume);
        PlayerPrefs.SetFloat("fx_volume", FxVolume);
        PlayerPrefs.Save();
    }

    private void Awake()
    {
        Instance = this;

        MusicVolume = PlayerPrefs.GetFloat("music_volume", 0.5f);
        FxVolume = PlayerPrefs.GetFloat("fx_volume", 0.8f);
}
}
