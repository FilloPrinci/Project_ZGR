using UnityEngine;

public class PlayerSoundManager : MonoBehaviour
{
    public bool humanPlayer = false;

    private VeichleSoundEffects soundEffects;

    public void SetVeichleSoundEffects(VeichleSoundEffects soundEffects)
    {
        this.soundEffects = soundEffects;
    }


    public void PlayCollisionEffect()
    {
        if(soundEffects!= null && humanPlayer)
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
}
