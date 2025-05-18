using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private RaceManager raceManager;

    private Vector2 moveInput;
    private bool isAccelerating;
    private bool isPressingStart;

    public float SteerInput => moveInput.x;
    public bool AccelerateInput => isAccelerating;

    public bool IsPressingStart => isPressingStart;

    
    private void Start()
    {
        raceManager = RaceManager.Instance;
    }

    public void OnSteer(InputAction.CallbackContext context)
    {
        if (context.performed || context.canceled)
        {
            float value = context.ReadValue<float>();
            Debug.Log("Steer: " + value);
            moveInput.x = value;
        }
    }

    public void OnAccelerate(InputAction.CallbackContext context)
    {
        isAccelerating = context.ReadValue<float>() > 0.1f;
    }

    public void OnSkip(InputAction.CallbackContext context)
    {
        isPressingStart = context.ReadValue<float>() > 0.1f;

        if (isPressingStart) {
            raceManager.OnSkip();
                
                }
    }

}
