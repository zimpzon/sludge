using System;
using UnityEngine;

[Serializable]
public class SoundItem
{
    public AudioClip[] clips;
    [Range(0f, 1f)]
    public float volume = 1;
    public float volumeVariation;
    [Range(0.1f, 3f)]
    public float pitch = 1;
    public float pitchVariation;
    public int maxInstances = 2;
    public float cooldown = 0.1f;
    public bool loop = false;
    public string comment;
}
