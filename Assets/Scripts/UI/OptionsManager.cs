using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class OptionsManager : MonoBehaviour
{
    public TMP_Dropdown inputSourceDropdown;
    public Toggle showFPSToggle;
    public Toggle useVsyncToggle;

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
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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
