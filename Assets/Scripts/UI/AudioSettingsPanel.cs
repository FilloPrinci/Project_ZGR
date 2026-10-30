using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Master / music / effects volume panel, opened from Configuration in the main menu. It has no
// input code of its own: UI_3D_Manager forwards the menu's navigation events here while the panel
// is open, so keyboard and gamepad behave the same.
public class AudioSettingsPanel : MonoBehaviour
{
    [Serializable]
    public class VolumeRow
    {
        public Image background;
        public Slider slider;
        public TextMeshProUGUI valueText;
    }

    public VolumeRow masterRow;
    public VolumeRow musicRow;
    public VolumeRow effectsRow;

    [Tooltip("Volume change for each left/right press.")]
    public float volumeStep = 0.1f;
    public Color selectedRowColor = new Color(1f, 1f, 1f, 0.15f);
    public Color unselectedRowColor = new Color(1f, 1f, 1f, 0f);

    [Tooltip("The main menu has no music: this plays while the music row is selected, so the player can hear the level being set.")]
    public AudioSource musicPreviewSource;

    private VolumeRow[] rows;
    private int selectedRowIndex;
    private float musicPreviewBaseVolume = -1f;

    public void Open()
    {
        rows = new VolumeRow[] { masterRow, musicRow, effectsRow };
        selectedRowIndex = 0;

        gameObject.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        StopMusicPreview();
        gameObject.SetActive(false);
    }

    public void SelectPrevious()
    {
        selectedRowIndex = (selectedRowIndex + rows.Length - 1) % rows.Length;
        Refresh();
    }

    public void SelectNext()
    {
        selectedRowIndex = (selectedRowIndex + 1) % rows.Length;
        Refresh();
    }

    // direction: +1 raises the selected volume, -1 lowers it.
    public void AdjustSelected(int direction)
    {
        GameSettings gameSettings = GameSettings.Instance;
        if (gameSettings == null)
        {
            return;
        }

        if (selectedRowIndex == 0)
        {
            gameSettings.masterVolume = Step(gameSettings.masterVolume, direction);
        }
        else if (selectedRowIndex == 1)
        {
            gameSettings.musicVolume = Step(gameSettings.musicVolume, direction);
        }
        else
        {
            gameSettings.effectsVolume = Step(gameSettings.effectsVolume, direction);
        }

        gameSettings.ApplyAudioSettings();
        Refresh();
    }

    // Rounded to the step so repeated presses can't accumulate float errors.
    private float Step(float volume, int direction)
    {
        return Mathf.Clamp01(Mathf.Round((volume + direction * volumeStep) / volumeStep) * volumeStep);
    }

    private void Refresh()
    {
        RefreshRow(masterRow, GameSettings.MasterVolumeFactor);
        RefreshRow(musicRow, GameSettings.MusicVolumeFactor);
        RefreshRow(effectsRow, GameSettings.EffectsVolumeFactor);

        for (int i = 0; i < rows.Length; i++)
        {
            rows[i].background.color = i == selectedRowIndex ? selectedRowColor : unselectedRowColor;
        }

        UpdateMusicPreview();
    }

    private void RefreshRow(VolumeRow row, float volume)
    {
        row.slider.value = volume;
        row.valueText.text = Mathf.RoundToInt(volume * 100f) + "%";
    }

    private void UpdateMusicPreview()
    {
        if (musicPreviewSource == null)
        {
            return;
        }

        if (musicPreviewBaseVolume < 0f)
        {
            musicPreviewBaseVolume = musicPreviewSource.volume;
        }

        musicPreviewSource.volume = musicPreviewBaseVolume * GameSettings.MusicVolumeFactor;

        if (rows[selectedRowIndex] != musicRow)
        {
            StopMusicPreview();
        }
        else if (!musicPreviewSource.isPlaying)
        {
            musicPreviewSource.Play();
        }
    }

    private void StopMusicPreview()
    {
        if (musicPreviewSource != null)
        {
            musicPreviewSource.Stop();
        }
    }
}
