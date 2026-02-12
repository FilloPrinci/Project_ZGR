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

        }
    }

    #endregion

    #endregion
}
