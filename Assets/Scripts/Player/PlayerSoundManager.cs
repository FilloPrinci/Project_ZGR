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

        engineAudioSource = soundEffects.engineEffect.audioSource;

        if (isHuman && soundEffects != null)
        {
            CustomAudioEffect collisionAudioEffect = soundEffects.collisionEffect;
        }
    }


    public void PlayCollisionEffect()
    {
        if(soundEffects!= null && isHuman)
        {
            //Debug.Log("PlayerSoundManager: Collision detected, playing sound effect.");
            // play collision sound effect (only for human players)

            CustomAudioEffect collisionAudioEffect = soundEffects.collisionEffect;

            collisionAudioEffect.audioSource.Stop();
            collisionAudioEffect.audioSource.time = 0f; // reset audio to the beginning
            collisionAudioEffect.audioSource.pitch = collisionAudioEffect.basePitch + Random.Range(0.0f, 1f); // add some random pitch variation for more realism
            collisionAudioEffect.audioSource.volume = collisionAudioEffect.baseVolume; // set volume to a reasonable level
            collisionAudioEffect.audioSource.Play();
        }
        
    }

    private void ApplyEngineAudioEffects(bool boostMode, float currentEnginePower, float deltaTime)
    {

        if (engineAudioSource != null)
        {
            float desideredAudioPitch = 1f; // Desired pitch based on engine power
            float desideredAudioVolume = 1f; // Desired volume based on engine power
            float baseAudioPitch = soundEffects.engineEffect.basePitch; // Base pitch for the engine sound
            float baseAudioVolume = soundEffects.engineEffect.baseVolume; // Base volume for the engine sound
            float engineAudioVolume = soundEffects.engineEffect.volumeFactor; // Volume factor for the engine sound

            if (!boostMode)
            {
                desideredAudioPitch = Mathf.Lerp(baseAudioPitch, baseAudioPitch * 2f, currentEnginePower); // Adjust the pitch range as needed
                desideredAudioVolume = Mathf.Lerp(baseAudioVolume * 0.5f, baseAudioVolume, currentEnginePower); // Adjust volume based on engine power
            }
            else
            {
                desideredAudioPitch = baseAudioPitch * 2f; // Max pitch in boost mode
                desideredAudioVolume = baseAudioVolume; // Max volume in boost mode
            }


            float currentAudioPitch = engineAudioSource.pitch;
            desideredAudioPitch = baseAudioPitch + currentEnginePower; // Adjust pitch based on boost mode and engine power
            desideredAudioPitch = Mathf.Lerp(currentAudioPitch, desideredAudioPitch, deltaTime * 5f); // Smoothly interpolate pitch

            engineAudioSource.pitch = desideredAudioPitch;

            float currentAudioVolume = engineAudioSource.volume;
            desideredAudioVolume = baseAudioVolume + (currentEnginePower * engineAudioVolume); // Adjust volume based on boost mode and engine power
            desideredAudioVolume = Mathf.Lerp(currentAudioVolume, desideredAudioVolume, deltaTime * 5f); // Smoothly interpolate volume

            engineAudioSource.volume = desideredAudioVolume;
        }
    }
}
