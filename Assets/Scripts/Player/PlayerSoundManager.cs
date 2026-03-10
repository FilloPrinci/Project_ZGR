using UnityEngine;

public class PlayerSoundManager : MonoBehaviour
{
    private bool isHuman = false;

    private VeichleSoundEffects soundEffects;

    private AudioSource engineAudioSource;
    private bool boostMode = false;
    private float currentEnginePower = 0f;
    private RaceManager raceManager;


    private void Start()
    {
        raceManager = RaceManager.Instance;

        if(raceManager == null)
        {
            Debug.LogWarning("PlayerSoundManager: RaceManager instance not found. Please ensure that a RaceManager is present in the scene.");
        }
    }

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

        int humanPlayersAmount = 1; // Default to 1 if raceManager is not available

        if (raceManager != null)
        {
            humanPlayersAmount = raceManager.GetHumanPlayersAmount();
        }

        if (this.soundEffects != null) {
            engineAudioSource = soundEffects.engineEffect.audioSource;

            if (!isHuman)
            {
                soundEffects.engineEffect.basePitch += Random.Range(-0.5f, 0.5f);
                soundEffects.engineEffect.baseVolume *= 0.5f;
                soundEffects.engineEffect.volumeFactor *= 0.5f;
                soundEffects.collisionEffect.baseVolume *= 0.75f;

                if(humanPlayersAmount > 1)
                {
                    soundEffects.engineEffect.audioSource.spatialBlend = 0.5f; // Set spatial blend to 1 for AI players (3D sound) if there are multiple human players
                    soundEffects.collisionEffect.audioSource.spatialBlend = 0.5f; // Set spatial blend to 1 for AI players (3D sound) if there are multiple human players
                    soundEffects.engineEffect.baseVolume *= 0.5f;
                    soundEffects.engineEffect.volumeFactor *= 0.5f;
                    soundEffects.collisionEffect.baseVolume *= 0.5f;
                    soundEffects.collisionEffect.audioSource.maxDistance = 0f; // Set max distance to 0 for human players to ensure consistent volume regardless of distance
                    soundEffects.engineEffect.audioSource.maxDistance = 0f; // Set max distance to 0 for human players to ensure consistent volume regardless of distance
                }
            }
            else
            { 
                soundEffects.collisionEffect.audioSource.spatialBlend = 0f; // Set spatial blend to 0 for human players (2D sound)
                soundEffects.collisionEffect.audioSource.maxDistance = 0f; // Set max distance to 0 for human players to ensure consistent volume regardless of distance
                soundEffects.engineEffect.audioSource.spatialBlend = 0f; // Set spatial blend to 0 for human players (2D sound)
                soundEffects.engineEffect.audioSource.maxDistance = 0f; // Set max distance to 0 for human players to ensure consistent volume regardless of distance
                soundEffects.collisionEffect.baseVolume *= 1 / humanPlayersAmount; // Adjust collision sound volume based on the number of human players to prevent overwhelming audio
                soundEffects.engineEffect.baseVolume *= 1 / humanPlayersAmount; // Adjust collision sound volume based on the number of human players to prevent overwhelming audio
                
            }
        }

    }


    public void PlayCollisionEffect(float impactPower = 1f)
    {
        if(soundEffects != null)
        {
            // play collision sound effect (only for human players)

            CustomAudioEffect collisionAudioEffect = soundEffects.collisionEffect;

            collisionAudioEffect.audioSource.pitch = collisionAudioEffect.basePitch + Random.Range(0.0f, 1f); // add some random pitch variation for more realism


            collisionAudioEffect.audioSource.PlayOneShot(collisionAudioEffect.audioSource.clip, collisionAudioEffect.baseVolume * impactPower); // play the collision sound with adjusted volume based on impact power
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
