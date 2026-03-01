using NUnit.Framework;
using UnityEngine;

[System.Serializable]
public class CustomAudioEffect{
    public AudioSource audioSource;
    public float basePitch = 1f;
    public float baseVolume = 1f;
}

public class VeichleSoundEffects : MonoBehaviour
{
    public CustomAudioEffect collisionEffect;
}
