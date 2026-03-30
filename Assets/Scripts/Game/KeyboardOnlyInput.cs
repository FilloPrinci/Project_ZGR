using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardOnlyInput : MonoBehaviour
{
    private UI_CustomInputManager UI_InputManagerInstance;
    private GlobalInputManager GlobalInputManagerInstance;

    public bool active = false;

    private void Awake()
    {
        active = GameSettings.Instance != null && GameSettings.Instance.inputMode == InputMode.KeyboardOnly && RaceSettings.Instance != null && RaceSettings.Instance.inputPlayersAmount > 1;
    }

    private void Start()
    {
        if (active)
        {
            GlobalInputManagerInstance = GlobalInputManager.Instance;

            if (GlobalInputManagerInstance == null)
            {
                Debug.LogWarning("GlobalInputManager instance not found. Please ensure it is present in the scene.");
            }
        }
        else { 
            GlobalInputManagerInstance = null;  
        }
    }

    public void Initialize(UI_CustomInputManager inputManagerInstance)
    {
        if (inputManagerInstance != null)
        {
            UI_InputManagerInstance = inputManagerInstance;
        }
        else
        {
            Debug.LogWarning("UI_CustomInputManager instance is null. Please provide a valid instance.");
        }
    }

    #region P1

    public void Player1_SelectHorizontal(InputAction.CallbackContext context)
    {

        if (context.performed)
        {

            float value = context.ReadValue<float>();
            if (value != 0)
            {
                if (value > 0)
                {
                    UI_InputManagerInstance.OnSelectRight(0);
                }
                else
                {
                    UI_InputManagerInstance.OnSelectLeft(0);
                }
            }
        }
    }

    public void Player1_SelectVerticall(InputAction.CallbackContext context)
    {
        if (context.performed)
        {

            float value = context.ReadValue<float>();
            if (value != 0)
            {
                if (value > 0)
                {
                    UI_InputManagerInstance.OnSelectUp(0);
                }
                else
                {
                    UI_InputManagerInstance.OnSelectDown(0);
                }
            }
        }
    }

    public void Player1_ConfirmSelection(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UI_InputManagerInstance.OnConfirmSelection(0);
        }
    }

    public void Player1_CancelSelection(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UI_InputManagerInstance.OnCancelSelection(0);
        }
    }

    public void Player1_OnSteer(InputAction.CallbackContext context)
    {
        if(GlobalInputManagerInstance != null)
        {
            GlobalInputManagerInstance.GlobalSteer(context, 0);
        }
        
    }

    public void Player1_OnAccelerate(InputAction.CallbackContext context)
    {
        if (GlobalInputManagerInstance != null)
        {
            GlobalInputManagerInstance.GlobalAccelerate(context, 0);
        }
    }

    public void Player1_OnStartPause(InputAction.CallbackContext context)
    {
        if (GlobalInputManagerInstance != null)
        {
            GlobalInputManagerInstance.GlobalStartPause(context, 0);
        }
    }

    #endregion

    #region P2

    public void Player2_SelectHorizontal(InputAction.CallbackContext context)
    {

        if (context.performed)
        {

            float value = context.ReadValue<float>();
            if (value != 0)
            {
                if (value > 0)
                {
                    UI_InputManagerInstance.OnSelectRight(1);
                }
                else
                {
                    UI_InputManagerInstance.OnSelectLeft(1);
                }
            }
        }
    }

    public void Player2_SelectVerticall(InputAction.CallbackContext context)
    {
        if (context.performed)
        {

            float value = context.ReadValue<float>();
            if (value != 0)
            {
                if (value > 0)
                {
                    UI_InputManagerInstance.OnSelectUp(1);
                }
                else
                {
                    UI_InputManagerInstance.OnSelectDown(1);
                }
            }
        }
    }

    public void Player2_ConfirmSelection(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UI_InputManagerInstance.OnConfirmSelection(1);
        }
    }

    public void Player2_CancelSelection(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UI_InputManagerInstance.OnCancelSelection(1);
        }
    }

    public void Player2_OnSteer(InputAction.CallbackContext context)
    {
        GlobalInputManagerInstance.GlobalSteer(context, 1);
    }

    public void Player2_OnAccelerate(InputAction.CallbackContext context)
    {
        GlobalInputManagerInstance.GlobalAccelerate(context, 1);
    }

    public void Player2_OnStartPause(InputAction.CallbackContext context)
    {
        if(GlobalInputManagerInstance != null)
        {
            GlobalInputManagerInstance.GlobalStartPause(context, 1);
        }
            
    }

    #endregion

    #region P3

    public void Player3_SelectHorizontal(InputAction.CallbackContext context)
    {

        if (context.performed)
        {

            float value = context.ReadValue<float>();
            if (value != 0)
            {
                if (value > 0)
                {
                    UI_InputManagerInstance.OnSelectRight(2);
                }
                else
                {
                    UI_InputManagerInstance.OnSelectLeft(2);
                }
            }
        }
    }

    public void Player3_SelectVerticall(InputAction.CallbackContext context)
    {
        if (context.performed)
        {

            float value = context.ReadValue<float>();
            if (value != 0)
            {
                if (value > 0)
                {
                    UI_InputManagerInstance.OnSelectUp(2);
                }
                else
                {
                    UI_InputManagerInstance.OnSelectDown(2);
                }
            }
        }
    }

    public void Player3_ConfirmSelection(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UI_InputManagerInstance.OnConfirmSelection(2);
        }
    }

    public void Player3_CancelSelection(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UI_InputManagerInstance.OnCancelSelection(2);
        }
    }

    public void Player3_OnSteer(InputAction.CallbackContext context)
    {
        GlobalInputManagerInstance.GlobalSteer(context, 2);
    }

    public void Player3_OnAccelerate(InputAction.CallbackContext context)
    {
        GlobalInputManagerInstance.GlobalAccelerate(context, 2);
    }

    public void Player3_OnStartPause(InputAction.CallbackContext context)
    {
        if (GlobalInputManagerInstance != null)
        {
            GlobalInputManagerInstance.GlobalStartPause(context, 2);
        }
            
    }

    #endregion

    #region P4

    public void Player4_SelectHorizontal(InputAction.CallbackContext context)
    {

        if (context.performed)
        {

            float value = context.ReadValue<float>();
            if (value != 0)
            {
                if (value > 0)
                {
                    UI_InputManagerInstance.OnSelectRight(3);
                }
                else
                {
                    UI_InputManagerInstance.OnSelectLeft(3);
                }
            }
        }
    }

    public void Player4_SelectVerticall(InputAction.CallbackContext context)
    {
        if (context.performed)
        {

            float value = context.ReadValue<float>();
            if (value != 0)
            {
                if (value > 0)
                {
                    UI_InputManagerInstance.OnSelectUp(3);
                }
                else
                {
                    UI_InputManagerInstance.OnSelectDown(3);
                }
            }
        }
    }

    public void Player4_ConfirmSelection(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UI_InputManagerInstance.OnConfirmSelection(3);
        }
    }

    public void Player4_CancelSelection(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UI_InputManagerInstance.OnCancelSelection(3);
        }
    }

    public void Player4_OnSteer(InputAction.CallbackContext context)
    {
        GlobalInputManagerInstance.GlobalSteer(context, 3);
    }

    public void Player4_OnAccelerate(InputAction.CallbackContext context)
    {
        GlobalInputManagerInstance.GlobalAccelerate(context, 3);
    }

    public void Player4_OnStartPause(InputAction.CallbackContext context)
    {
        if (GlobalInputManagerInstance != null)
        {
            GlobalInputManagerInstance.GlobalStartPause(context, 3);
        }
            
    }

    #endregion

}
