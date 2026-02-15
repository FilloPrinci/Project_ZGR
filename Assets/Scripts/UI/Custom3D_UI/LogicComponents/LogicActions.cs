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

    public void QuitGame()
    {
        Application.Quit();
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
            gameSettings.resolution = Resolutions.HD;
            gameSettings.ApplyVideoSettings();
        }
    }

    public void SetResolutionFullHD()
    {
        if (gameSettings != null)
        {
            gameSettings.resolution = Resolutions.Full_HD;
            gameSettings.ApplyVideoSettings();
        }
    }

    public void SetResolutionQHD()
    {
        if (gameSettings != null)
        {
            gameSettings.resolution = Resolutions.QHD;
            gameSettings.ApplyVideoSettings();
        }
    }

    public void SetResolutionUHD()
    {
        if (gameSettings != null)
        {
            gameSettings.resolution = Resolutions.UHD;
            gameSettings.ApplyVideoSettings();
        }
    }

    public void SetFramerateUnlock()
    {
        if (gameSettings != null)
        {
            gameSettings.fps_settings = FPS_Settings.free;
            gameSettings.ApplyVideoSettings();
        }
    }

    public void SetFramerateVSync()
    {
        if (gameSettings != null)
        {
            gameSettings.fps_settings = FPS_Settings.VSync;
            gameSettings.ApplyVideoSettings();
        }
    }

    public void SetFramerate120()
    {
        if (gameSettings != null)
        {
            gameSettings.fps_settings = FPS_Settings.Hz120;
            gameSettings.ApplyVideoSettings();
        }
    }

    public void SetFramerate60()
    {
        if (gameSettings != null)
        {
            gameSettings.fps_settings = FPS_Settings.Hz60;
            gameSettings.ApplyVideoSettings();
        }
    }

    public void SetFramerate30()
    {
        if (gameSettings != null)
        {
            gameSettings.fps_settings = FPS_Settings.Hz30;
            gameSettings.ApplyVideoSettings();
        }
    }

    public void SetQualityLow()
    {
        if (gameSettings != null)
        {
            gameSettings.qualitySettings = QualityLevel.Low;
            gameSettings.ApplyVideoSettings();
        }
    }

    public void SetQualityMedium()
    {
        if (gameSettings != null)
        {
            gameSettings.qualitySettings = QualityLevel.Medium;
            gameSettings.ApplyVideoSettings();
        }
    }

    public void SetQualityHigh()
    {
        if (gameSettings != null)
        {
            gameSettings.qualitySettings = QualityLevel.High;
            gameSettings.ApplyVideoSettings();
        }
    }

    #endregion

    #endregion
}
