using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputMode
{
    Both,
    GamepadOnly,
    KeyboardOnly
}

public enum FPSLimit 
{
    None,
    Limit_30,
    Limit_40,
    Limit_60,
    Limit_90,
    Limit_120
}

public enum Resolutions
{
    HD,
    Full_HD,
    QHD,
    UHD
}

public enum FPS_Settings
{
    VSync,
    free,
    Hz120,
    Hz60,
    Hz30
}

public enum QualityLevel
{
    Low,
    Medium,
    High,
}

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    // Input Settings
    [Header("Input")]
    public InputMode inputMode;
    // TODO: to be implemented
    public bool useCustomMapping = false;


    // Video Settings
    [Header("Video")]
    public bool showFPS = false;
    public FPSLimit fPSLimit;
    public bool useV_Sync = false;

    // TODO: to be implemented
    //public Resolution resolution = Resolution.1920_1080;
    // TODO: to be implemented
    //public bool bloom = true;

    public Resolutions resolution;
    public FPS_Settings fps_settings;
    public QualityLevel qualitySettings;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
            ApplyVideoSettings();

            if (inputMode == InputMode.GamepadOnly && !IsGamepadAvailable())
            {
                Debug.LogWarning("Gamepad mode selected but no gamepads detected. Defaulting to Keyboard.");
                inputMode = InputMode.KeyboardOnly;
            }
        }
        else
        {
            Debug.LogWarning("Duplicate GameSettings detected. Destroying extra instance.");
            Destroy(gameObject); // Assicura che ci sia solo un GameSettings
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //ApplySettings();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool IsGamepadAvailable()
    {
        return Gamepad.all.Count > 0;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("InputMode", (int)inputMode);
        PlayerPrefs.SetInt("UseCustomMapping", useCustomMapping ? 1 : 0);

        PlayerPrefs.SetInt("Resolution", (int)resolution);
        PlayerPrefs.SetInt("FPS_Settings", (int)fps_settings);
        PlayerPrefs.SetInt("QualitySettings", (int)qualitySettings);

        PlayerPrefs.Save(); // Salva su disco
    }

    public void LoadSettings()
    {
        //inputMode = (InputMode)PlayerPrefs.GetInt("InputMode", 0);
        useCustomMapping = PlayerPrefs.GetInt("UseCustomMapping", 0) == 1;
        showFPS = PlayerPrefs.GetInt("ShowFPS", 0) == 1;
        inputMode = (InputMode)PlayerPrefs.GetInt("InputMode", 0);

        // default is Full HD
        resolution = (Resolutions)PlayerPrefs.GetInt("Resolution", 1);
        // default is VSync
        fps_settings = (FPS_Settings)PlayerPrefs.GetInt("FPS_Settings", 1);
        // default is High
        qualitySettings = (QualityLevel)PlayerPrefs.GetInt("QualitySettings", 2);
    }

    [Obsolete("Use ApplyVideoSettings instead for video settings and ApplyInputSettings for input settings.")]
    public void ApplySettings()
    {
        // Apply V-Sync
        QualitySettings.vSyncCount = useV_Sync ? 1 : 0;

        // Apply FPS limit only if V-Sync is disabled
        if (!useV_Sync)
        {
            switch (fPSLimit)
            {
                case FPSLimit.None:
                    Application.targetFrameRate = -1; // No Limit
                    Time.maximumDeltaTime = 1f / 120;
                    Time.fixedDeltaTime = 1f / 120;
                    break;
                case FPSLimit.Limit_30:
                    Time.maximumDeltaTime = 1f / 30;
                    Time.fixedDeltaTime = 1f / 30;
                    Application.targetFrameRate = 30;
                    break;
                case FPSLimit.Limit_40:
                    Time.maximumDeltaTime = 1f / 40;
                    Time.fixedDeltaTime = 1f / 40;
                    Application.targetFrameRate = 40;
                    break;
                case FPSLimit.Limit_60:
                    Time.maximumDeltaTime = 1f / 60;
                    Time.fixedDeltaTime = 1f / 60;
                    Application.targetFrameRate = 60;
                    break;
                case FPSLimit.Limit_90:
                    Time.maximumDeltaTime = 1f / 90;
                    Time.fixedDeltaTime = 1f / 90;
                    Application.targetFrameRate = 90;
                    break;
                case FPSLimit.Limit_120:
                    Time.maximumDeltaTime = 1f / 120;
                    Time.fixedDeltaTime = 1f / 120;
                    Application.targetFrameRate = 120;
                    break;
            }
        }
        else
        {
            Application.targetFrameRate = -1;
        }

        Debug.Log($"Settings Applied: VSync = {QualitySettings.vSyncCount}, Target FPS = {Application.targetFrameRate}");
    }

    public void ApplyInputSettings(UI_CustomInputManager inputManager)
    {
        Debug.Log("Applying new input settings...");

        if (inputManager != null)
        {
            inputManager.UpdateInputSettings();
        }

        Debug.Log("InputSettings applyed (" + inputMode + ")");

       SaveSettings();
    }

    public void ApplyVideoSettings()
    {

        Vector2 resolutionInfo = ResolutionFromEnum(this.resolution);

        Screen.SetResolution((int)resolutionInfo.x, (int)resolutionInfo.y, Screen.fullScreenMode, 30);


        int framerate = RefreshRateFromEnum(this.fps_settings);

        if(framerate == 0)
        {
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            QualitySettings.vSyncCount = 0;

            if (framerate == -1) {
                Application.targetFrameRate = -1;
            }
            else
            {
                Application.targetFrameRate = framerate;
            }
        }

        QualitySettings.SetQualityLevel((int)this.qualitySettings, true);
        SaveSettings();

    }


    private Vector2 ResolutionFromEnum(Resolutions resolution)
    {
        if(resolution == Resolutions.HD)
        {
            return new Vector2(1280, 720);
        }else if (resolution == Resolutions.Full_HD)
        {
            return new Vector2(1920, 1080);
        }else if (resolution == Resolutions.QHD)
        {
            return new Vector2(2560, 1440);
        }else if (resolution == Resolutions.UHD)
        {
            return new Vector2(3840, 2160);
        }
        else
        {
            return new Vector2(1280, 720);
        }
    }

    private int RefreshRateFromEnum(FPS_Settings fps_settings) {
        if (fps_settings == FPS_Settings.free)
        {
            return -1;
        }
        else if (fps_settings == FPS_Settings.VSync)
        {
            return 0;
        }
        else if (fps_settings == FPS_Settings.Hz120)
        {
            return 120;
        }
        else if (fps_settings == FPS_Settings.Hz60)
        {
            return 60;
        }
        else if (fps_settings == FPS_Settings.Hz30)
        {
            return 30;
        }
        else
        {
            return 0;
        }
    }

}
