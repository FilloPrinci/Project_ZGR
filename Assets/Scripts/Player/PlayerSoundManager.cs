using UnityEngine;

public class PlayerSoundManager : MonoBehaviour
{
    private bool isHuman = false;

    private VeichleSoundEffects soundEffects;

    private AudioSource engineAudioSource;
    private bool boostMode = false;
    private float currentEnginePower = 0f;


    private void LateUpdate()
    {
        if (soundEffects != null)
        {
            ApplyEngineAudioEffects(boostMode, currentEnginePower, Time.deltaTime);
        }
    }

    public void UpdateEngineEffect(bool isBoostMode, float enginePower)
    {
        boostMode = isBoostMode;
        currentEnginePower = enginePower;
    }

    public void Setup(bool isHuman, VeichleSoundEffects soundEffects)
    {
        this.isHuman = isHuman;
        this.soundEffects = soundEffects;

        if (this.soundEffects != null) {
            engineAudioSource = soundEffects.engineEffect.audioSource;

            if (!isHuman)
            {
                soundEffects.engineEffect.basePitch += Random.Range(-0.5f, 0.5f);
                soundEffects.engineEffect.baseVolume *= 0.5f;
                soundEffects.engineEffect.volumeFactor *= 0.5f;
                soundEffects.collisionEffect.baseVolume *= 0.75f;
            }
        }

    }


    public void PlayCollisionEffect(float impactPower = 1f)
    {
        if(soundEffects != null)
        {
            Debug.Log("PlayerSoundManager: Collision detected, playing sound effect.");
            // play collision sound effect (only for human players)

            CustomAudioEffect collisionAudioEffect = soundEffects.collisionEffect;

            collisionAudioEffect.audioSource.Stop();
            collisionAudioEffect.audioSource.time = 0f; // reset audio to the beginning
            collisionAudioEffect.audioSource.pitch = collisionAudioEffect.basePitch + Random.Range(0.0f, 1f); // add some random pitch variation for more realism
            collisionAudioEffect.audioSource.volume = collisionAudioEffect.baseVolume * impactPower; // set volume to a reasonable level
            collisionAudioEffect.audioSource.Play();
        }
        
    }

    private void ApplyEngineAudioEffects(bool boostMode, float currentEnginePower, float deltaTime)
    {

        if (engineAudioSource != null)
        {
            float baseAudioPitch = soundEffects.engineEffect.basePitch; // Base pitch for the engine sound
            float baseAudioVolume = soundEffects.engineEffect.baseVolume; // Base volume for the engine sound
            float engineAudioVolume = soundEffects.engineEffect.volumeFactor; // Volume factor for the engine sound

            if (!boostMode)
            {
                soundEffects.engineEffect.desideredPitch = Mathf.Lerp(baseAudioPitch, baseAudioPitch * 2f, currentEnginePower); // Adjust the pitch range as needed
                soundEffects.engineEffect.desideredVolume = Mathf.Lerp(baseAudioVolume * 0.5f, baseAudioVolume, currentEnginePower); // Adjust volume based on engine power
            }
            else
            {
                soundEffects.engineEffect.desideredPitch = baseAudioPitch * 2f; // Max pitch in boost mode
                soundEffects.engineEffect.desideredVolume = baseAudioVolume; // Max volume in boost mode
            }


            float currentAudioPitch = engineAudioSource.pitch;
            soundEffects.engineEffect.desideredPitch = baseAudioPitch + currentEnginePower; // Adjust pitch based on boost mode and engine power
            soundEffects.engineEffect.desideredPitch = Mathf.Lerp(currentAudioPitch, soundEffects.engineEffect.desideredPitch, deltaTime * 5f); // Smoothly interpolate pitch

            engineAudioSource.pitch = soundEffects.engineEffect.desideredPitch;

            float currentAudioVolume = engineAudioSource.volume;
            soundEffects.engineEffect.desideredVolume = baseAudioVolume + (currentEnginePower * engineAudioVolume); // Adjust volume based on boost mode and engine power
            soundEffects.engineEffect.desideredVolume = Mathf.Lerp(currentAudioVolume, soundEffects.engineEffect.desideredVolume, deltaTime * 5f); // Smoothly interpolate volume

            engineAudioSource.volume = soundEffects.engineEffect.desideredVolume;
        }
    }
}
