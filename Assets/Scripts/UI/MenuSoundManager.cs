using UnityEngine;

public class MenuSoundManager : MonoBehaviour
{
    public MenuSoundEffects soundEffects;

    private void OnValidate()
    {
        if(soundEffects == null)
        {
            Debug.LogWarning("MenuSoundManager: SoundEffects reference is not set. Please assign a MenuSoundEffects asset to the MenuSoundManager.");
        }
        else
        {
            soundEffects.selectionEffect.audioSource.volume = soundEffects.selectionEffect.baseVolume;
            soundEffects.confirmEffect.audioSource.volume = soundEffects.confirmEffect.baseVolume;
            soundEffects.backEffect.audioSource.volume = soundEffects.backEffect.baseVolume;

        }
    }

    public void PlaySelectionEffect()
    {
        if (soundEffects != null && soundEffects.selectionEffect != null)
        {
            soundEffects.selectionEffect.audioSource.PlayOneShot(soundEffects.selectionEffect.audioSource.clip);
        }
    }

    public void PlayConfirmEffect()
    {
        if (soundEffects != null && soundEffects.confirmEffect != null)
        {
            soundEffects.confirmEffect.audioSource.PlayOneShot(soundEffects.confirmEffect.audioSource.clip);
        }
    }

    public void PlayBackEffect()
    {
        if (soundEffects != null && soundEffects.backEffect != null)
        {
            soundEffects.backEffect.audioSource.PlayOneShot(soundEffects.backEffect.audioSource.clip);
        }
    }
}
