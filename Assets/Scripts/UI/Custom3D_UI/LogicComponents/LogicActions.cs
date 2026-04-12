using UnityEngine;
using UnityEngine.InputSystem;

public class LogicActions : MonoBehaviour
{

    public UI_CustomInputManager uI_CustomInputManager;

    private GameSettings gameSettings;
    private RaceSettings raceSettings;
    private UI_3D_Manager uI_3D_Manager;


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
        raceSettings = RaceSettings.Instance;
        uI_3D_Manager = UI_3D_Manager.Instance;

        if (gameSettings == null)
        {
            Debug.LogWarning("GameSettings instance not found!");
        }

        if(raceSettings == null)
        {
            Debug.LogWarning("RaceSettings instance not found!");
        }

        if (uI_3D_Manager == null)
        {
            Debug.LogWarning("UI_3D_Manager instance not found!");
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

    #region Difficulty
    
    public void OnEasy()
    {
        raceSettings.SetSelectedDifficulty(GlobalDifficulty.easy);
    }

    public void OnNormal()
    {
        raceSettings.SetSelectedDifficulty(GlobalDifficulty.normal);
    }

    public void OnHard()
    {
        raceSettings.SetSelectedDifficulty(GlobalDifficulty.hard);
    }

    #endregion

    #region GameSettings

    #region Input

    public void SetInputModeGamepad()
    {
        bool completed = false;

        int gamepadAmount = Gamepad.all.Count;

        if (gamepadAmount > 0)
        {
            if (gameSettings != null)
            {
                gameSettings.inputMode = InputMode.GamepadOnly;
                gameSettings.ApplyInputSettings(uI_CustomInputManager);

                completed = true;
            }
        }
        else
        {
            Debug.LogWarning("No gamepad detected!");

            completed = false;
        }

        if (completed)
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", "gamepad is now in use"));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", "no gamepad detected"));
        }
    }

    public void SetInputModeKeyboard()
    {
        bool completed = false;

        if (gameSettings != null)
        {
            gameSettings.inputMode = InputMode.KeyboardOnly;
            gameSettings.ApplyInputSettings(uI_CustomInputManager);
            completed = true;
        }

        if (completed)
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", "keyboard is now in use"));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", ""));
        }

        
    }

    #endregion

    #region Video

    public void SetResolutionHD()
    {
        bool completed = false;

        if (gameSettings != null)
        {
            gameSettings.resolution = Resolutions.HD;
            gameSettings.ApplyVideoSettings();
            completed = true;
        }
        if (completed)
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", ""));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", ""));
        }
    }

    public void SetResolutionFullHD()
    {
        bool completed = false;

        if (gameSettings != null)
        {
            gameSettings.resolution = Resolutions.Full_HD;
            gameSettings.ApplyVideoSettings();
            completed = true;
        }
        if (completed)
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", ""));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", ""));
        }
    }

    public void SetResolutionQHD()
    {
        bool completed = false;

        if (gameSettings != null)
        {
            gameSettings.resolution = Resolutions.QHD;
            gameSettings.ApplyVideoSettings();
            completed = true;
        }
        if (completed)
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", ""));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", ""));
        }
    }

    public void SetResolutionUHD()
    {
        bool completed = false;

        if (gameSettings != null)
        {
            gameSettings.resolution = Resolutions.UHD;
            gameSettings.ApplyVideoSettings();
            completed = true;
        }
        if (completed)
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", ""));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", ""));
        }
    }

    public void SetFramerateUnlock()
    {
        bool completed = false;

        if (gameSettings != null)
        {
            gameSettings.fps_settings = FPS_Settings.free;
            gameSettings.ApplyVideoSettings();
            completed = true;
        }
        if (completed)
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", ""));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", ""));
        }
    }

    public void SetFramerateVSync()
    {
        bool completed = false;

        if (gameSettings != null)
        {
            gameSettings.fps_settings = FPS_Settings.VSync;
            gameSettings.ApplyVideoSettings();
            completed = true;
        }
        if (completed)
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", ""));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", ""));
        }
    }

    public void SetFramerate120()
    {
        bool completed = false;

        if (gameSettings != null)
        {
            gameSettings.fps_settings = FPS_Settings.Hz120;
            gameSettings.ApplyVideoSettings();
            completed = true;
        }
        if (completed)
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", ""));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", ""));
        }
    }

    public void SetFramerate60()
    {
        bool completed = false;

        if (gameSettings != null)
        {
            gameSettings.fps_settings = FPS_Settings.Hz60;
            gameSettings.ApplyVideoSettings();
            completed = true;
        }
        if (completed)
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", ""));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", ""));
        }
    }

    public void SetFramerate30()
    {
        bool completed = false;

        if (gameSettings != null)
        {
            gameSettings.fps_settings = FPS_Settings.Hz30;
            gameSettings.ApplyVideoSettings();
            completed = true;
        }
        if (completed)
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", ""));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", ""));
        }
    }

    public void SetQualityLow()
    {
        bool completed = false;

        if (gameSettings != null)
        {
            gameSettings.qualitySettings = QualityLevel.Low;
            gameSettings.ApplyVideoSettings();
            completed = true;
        }

    }

    public void SetQualityMedium()
    {
        bool completed = false;

        if (gameSettings != null)
        {
            gameSettings.qualitySettings = QualityLevel.Medium;
            gameSettings.ApplyVideoSettings();
            completed = true;
        }
        if (completed)
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", ""));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", ""));
        }
    }

    public void SetQualityHigh()
    {
        bool completed = false;

        if (gameSettings != null)
        {
            gameSettings.qualitySettings = QualityLevel.High;
            gameSettings.ApplyVideoSettings();
            completed = true;
        }
        if (completed)
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", ""));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", ""));
        }
    }

    #endregion

    #endregion
}
