using System;
using UnityEngine;

[Serializable]
public class SoundItem
{
    public AudioClip[] clips;
    [Range(0f, 1f)]
    public float volume = 1;
    public float volumeVariation;
    [Range(0.1f, 5f)]
    public float pitch = 10;
    public float pitchVariation;
    public bool loop = false;
    public float cooldown;
    public string comment;

    [NonSerialized] public AudioSource audioSource;
    [NonSerialized] public float timeLastPlayed;
}
