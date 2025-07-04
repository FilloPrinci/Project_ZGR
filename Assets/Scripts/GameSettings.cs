using UnityEngine;

public enum InputMode
{
    KeyboardOnly,
    GamepadOnly,
    Both
}

public enum FPSLimit 
{
    None,
    Limit_30,
    Limit_40,
    Limit_60,
    Limit_120
}

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    // Input Settings

    public InputMode inputMode;
    // TODO: to be implemented
    public bool useCustomMapping = false;


    // Video Settings

    public bool showFPS = false;
    public FPSLimit fPSLimit;
    public bool useV_Sync = false;

    // TODO: to be implemented
    //public Resolution resolution = Resolution.1920_1080;
    // TODO: to be implemented
    //public bool bloom = true;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Debug.LogError("Duplicate GameSettings detected. Destroying extra instance.");
            Destroy(gameObject); // Assicura che ci sia solo un GameSettings
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ApplySettings();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("InputMode", (int)inputMode);
        PlayerPrefs.SetInt("UseCustomMapping", useCustomMapping ? 1 : 0);
        PlayerPrefs.SetInt("ShowFPS", showFPS ? 1 : 0);
        PlayerPrefs.SetInt("FPSLimit", (int)fPSLimit);
        PlayerPrefs.SetInt("UseVSync", useV_Sync ? 1 : 0);

        PlayerPrefs.Save(); // Salva su disco
    }

    public void LoadSettings()
    {
        inputMode = (InputMode)PlayerPrefs.GetInt("InputMode", 0);
        useCustomMapping = PlayerPrefs.GetInt("UseCustomMapping", 0) == 1;
        showFPS = PlayerPrefs.GetInt("ShowFPS", 0) == 1;
        fPSLimit = (FPSLimit)PlayerPrefs.GetInt("FPSLimit", 0);
        useV_Sync = PlayerPrefs.GetInt("UseVSync", 0) == 1;
    }

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
                    break;
                case FPSLimit.Limit_30:
                    Application.targetFrameRate = 30;
                    break;
                case FPSLimit.Limit_40:
                    Application.targetFrameRate = 40;
                    break;
                case FPSLimit.Limit_60:
                    Application.targetFrameRate = 60;
                    break;
                case FPSLimit.Limit_120:
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
}
