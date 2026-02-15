using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class OptionsManager : MonoBehaviour
{
    public TMP_Dropdown inputSourceDropdown;
    public Toggle showFPSToggle;
    public Toggle useVsyncToggle;

    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;
    private int currentResolutionIndex = 0;

    private GameSettings gameSettings;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameSettings = GameSettings.Instance;

        if(gameSettings == null)
        {
            Debug.LogError("GameSettings is not available in the scene. Make sure an GameSettings exists.");
            return;
        }
        else
        {
            inputSourceDropdown.value = (int)gameSettings.inputMode;
            inputSourceDropdown.RefreshShownValue();
            showFPSToggle.isOn = gameSettings.showFPS;
            useVsyncToggle.isOn = gameSettings.useV_Sync;


            // Ottiene tutte le risoluzioni supportate
            resolutions = Screen.resolutions;

            resolutionDropdown.ClearOptions();

            // Crea la lista di stringhe per il dropdown
            var options = new System.Collections.Generic.List<string>();

            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height ;
                options.Add(option);

                // Rileva la risoluzione attuale
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
            resolutionDropdown.RefreshShownValue();

            ApplySavedResolution();
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution res = resolutions[resolutionIndex];
        Screen.SetResolution(res.width, height: res.height, fullscreenMode: Screen.fullScreenMode, preferredRefreshRate: res.refreshRate);

        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.Save();
    }

    private void ApplySavedResolution()
    {
        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        SetResolution(savedIndex);
    }


    public void SetinputSource(int inputSource)
    {
        gameSettings.inputMode = (InputMode)inputSource;
    }

    public void ShowFPS(bool show)
    {
        gameSettings.showFPS = show;
    }

    public void UseVSync(bool vSync)
    {
        gameSettings.useV_Sync = vSync;
    }

    public void SetFPSCap(int fpsCap)
    {
        gameSettings.fPSLimit = (FPSLimit)fpsCap;
    }
    public void SetQualityLevel(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
    }

    public void OnApply()
    {
        gameSettings.SaveSettings();
        gameSettings.ApplySettings();
    }

    public void OnCancel()
    {
        gameSettings.LoadSettings();
        gameSettings.ApplySettings();
    }
}
