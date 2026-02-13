using UnityEngine;
using UnityEngine.InputSystem;

public class LogicActions : MonoBehaviour
{
    public UI_CustomInputManager uI_CustomInputManager;

    private GameSettings gameSettings;
    

    private void OnValidate()
    {
        if (uI_CustomInputManager == null)
        {
            Debug.LogError("uI_CustomInputManager not assigned!");
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameSettings = GameSettings.Instance;

        if(gameSettings == null)
        {
            Debug.LogWarning("GameSettings instance not found!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Test()
    {
        Debug.Log("TEST");
    }


    #region GameSettings

    #region Input

    public void SetInputModeGamepad()
    {
        int gamepadAmount = Gamepad.all.Count;

        if (gamepadAmount > 0)
        {
            if (gameSettings != null)
            {
                gameSettings.inputMode = InputMode.GamepadOnly;
                gameSettings.ApplyInputSettings(uI_CustomInputManager);
            }
        }
        else
        {
            Debug.LogWarning("No gamepad detected!");
        }
    }

    public void SetInputModeKeyboard()
    {
        if (gameSettings != null)
        {
            gameSettings.inputMode = InputMode.KeyboardOnly;
            gameSettings.ApplyInputSettings(uI_CustomInputManager);
        }
    }

    #endregion

    #region Video

    public void SetResolutionHD()
    {
        if(gameSettings != null)
        {
            gameSettings.ApplyVideoSettings(resolution: Resolutions.HD);
        }
    }

    public void SetResolutionFullHD()
    {
        if (gameSettings != null)
        {
            gameSettings.ApplyVideoSettings(resolution: Resolutions.Full_HD);
        }
    }

    public void SetResolutionQHD()
    {
        if (gameSettings != null)
        {
            gameSettings.ApplyVideoSettings(resolution: Resolutions.QHD);
        }
    }

    public void SetResolutionUHD()
    {
        if (gameSettings != null)
        {
            gameSettings.ApplyVideoSettings(resolution: Resolutions.UHD);
        }
    }

    public void SetFramerateUnlock()
    {
        if (gameSettings != null)
        {
            gameSettings.ApplyVideoSettings(fps_settings: FPS_Settings.free);
        }
    }

    public void SetFramerateVSync()
    {
        if (gameSettings != null)
        {
            gameSettings.ApplyVideoSettings(fps_settings: FPS_Settings.VSync);
        }
    }

    public void SetFramerate120()
    {
        if (gameSettings != null)
        {
            gameSettings.ApplyVideoSettings(fps_settings: FPS_Settings.Hz120);
        }
    }

    public void SetFramerate60()
    {
        if (gameSettings != null)
        {
            gameSettings.ApplyVideoSettings(fps_settings: FPS_Settings.Hz60);
        }
    }

    public void SetFramerate30()
    {
        if (gameSettings != null)
        {
            gameSettings.ApplyVideoSettings(fps_settings: FPS_Settings.Hz30);
        }
    }

    public void SetQualityLow()
    {
        if (gameSettings != null)
        {
            gameSettings.ApplyVideoSettings(qualitySettings: QualityLevel.Low);
        }
    }

    public void SetQualityMedium()
    {
        if (gameSettings != null)
        {
            gameSettings.ApplyVideoSettings(qualitySettings: QualityLevel.Medium);
        }
    }

    public void SetQualityHigh()
    {
        if (gameSettings != null)
        {
            gameSettings.ApplyVideoSettings(qualitySettings: QualityLevel.High);
        }
    }

    #endregion

    #endregion
}
