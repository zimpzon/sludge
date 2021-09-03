using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    AudioSource audioSource;

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

        Instance.audioSource.PlayOneShot(item.clips[0]);
    }

    private void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
    }
}
