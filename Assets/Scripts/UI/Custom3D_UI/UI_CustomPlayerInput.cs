using UnityEngine;
using UnityEngine.InputSystem;

public class UI_CustomPlayerInput : MonoBehaviour
{
    public GameObject UI_InputManager;
    private UI_CustomInputManager UI_InputManagerInstance;
    public int playerIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Initialize()
    {
        if (UI_InputManagerInstance == null)
        {
            UI_InputManagerInstance = UI_InputManager.GetComponent<UI_CustomInputManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Select(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            float value = context.ReadValue<float>();
            if (value != 0)
            {
                if(value > 0)
                {
                    UI_InputManagerInstance.OnSelectRight(playerIndex);
                }
                else
                {
                    UI_InputManagerInstance.OnSelectLeft(playerIndex);
                }
            }
        }
    }

    public void ConfirmSelection(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UI_InputManagerInstance.OnConfirmSelection(playerIndex);
        }
    }

    public void CancelSelection(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UI_InputManagerInstance.OnCancelSelection(playerIndex);
        }
    }
}
