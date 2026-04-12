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

    public void UI_Back()
    {
        uI_3D_Manager.ManageBackSelection(0);
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

    public void ShowInputLayout()
    {
        if(gameSettings.inputMode == InputMode.GamepadOnly)
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("Input Layout", "Gamepad layout:", image: uI_3D_Manager.controllerLayoutImage,  onClose: UI_Back));
        }
        else if(gameSettings.inputMode == InputMode.KeyboardOnly)
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("Input Layout", "Keyboard layout:", image: uI_3D_Manager.keyboardLayoutImage, onClose: UI_Back));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("Input Layout", "No input mode selected.", onClose: UI_Back));
        }
    }

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
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", "gamepad is now in use", onClose: UI_Back));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", "no gamepad detected", onClose: UI_Back));
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
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", "keyboard is now in use", onClose: UI_Back));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", "", onClose: UI_Back));
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
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", "", onClose: UI_Back));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", "", onClose: UI_Back));
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
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", "", onClose: UI_Back));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", "", onClose: UI_Back));
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
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", "", onClose: UI_Back));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", "", onClose: UI_Back));
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
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", "", onClose: UI_Back));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", "", onClose: UI_Back));
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
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", "", onClose: UI_Back));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", "", onClose: UI_Back));
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
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", "", onClose: UI_Back));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", "", onClose: UI_Back));
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
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", "", onClose: UI_Back));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", "", onClose: UI_Back));
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
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", "", onClose: UI_Back));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", "", onClose: UI_Back));
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
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", "", onClose: UI_Back));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", "", onClose: UI_Back));
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
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", "", onClose: UI_Back));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", "", onClose: UI_Back));
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
            uI_3D_Manager.ShowPopup(new PopupInfo("Done", "", onClose: UI_Back));
        }
        else
        {
            uI_3D_Manager.ShowPopup(new PopupInfo("ERROR", "", onClose: UI_Back));
        }
    }

    #endregion

    #endregion
}
