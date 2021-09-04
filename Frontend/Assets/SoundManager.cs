using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

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
        item.audioSource.clip = clip;
        item.audioSource.Play();
        item.timeLastPlayed = Time.time;
    }

    private void Awake()
    {
        Instance = this;
    }
}
