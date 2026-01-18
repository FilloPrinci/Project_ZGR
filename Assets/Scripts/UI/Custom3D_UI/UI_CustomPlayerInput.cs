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

    public void SelectRight(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UI_InputManagerInstance.OnSelectRight(playerIndex);
        }
    }
}
