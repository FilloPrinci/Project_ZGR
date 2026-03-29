using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardOnlyInput : MonoBehaviour
{
    private UI_CustomInputManager UI_InputManagerInstance;
    private GlobalInputManager GlobalInputManagerInstance;


    private void Start()
    {
        GlobalInputManagerInstance = GlobalInputManager.Instance;

        if( GlobalInputManagerInstance == null)
        {
            Debug.LogWarning("GlobalInputManager instance not found. Please ensure it is present in the scene.");
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
        GlobalInputManagerInstance.GlobalSteer(context, 0);
    }
    public void Player1_OnAccelerate(InputAction.CallbackContext context)
    {
        GlobalInputManagerInstance.GlobalAccelerate(context, 0);
    }

    public void Player1_OnStartPause(InputAction.CallbackContext context)
    {
        GlobalInputManagerInstance.GlobalStartPause(context, 0);
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
        GlobalInputManagerInstance.GlobalStartPause(context, 1);
    }

    #endregion

}
