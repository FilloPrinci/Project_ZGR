using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private RaceManager raceManager;

    private Vector2 moveInput;
    private bool isAccelerating;
    private bool isPressingStart;

    private int playerIndex = 0;

    public float SteerInput => moveInput.x;
    public bool AccelerateInput => isAccelerating;

    public bool IsPressingStart => isPressingStart;

    
    private void Start()
    {
        raceManager = RaceManager.Instance;
    }

    public void SetPlauerIndex(int index)
    {
        playerIndex = index;
    }

    public void OnSteer(InputAction.CallbackContext context)
    {
        if (context.performed || context.canceled)
        {
            float value = context.ReadValue<float>();
            moveInput.x = value;
        }
    }

    public void OnCPUSteer(float value)
    {
        moveInput.x = value;
    }


    public void OnCPUAccelerate(float accellerate)
    {
        isAccelerating = accellerate > 0.1f;
    }

    public void OnAccelerate(InputAction.CallbackContext context)
    {
        isAccelerating = context.ReadValue<float>() > 0.1f;
    }

    public void OnSkip(InputAction.CallbackContext context)
    {
        isPressingStart = context.ReadValue<float>() > 0.1f;

        if (isPressingStart) {
            raceManager.OnSkip(playerIndex);
                
        }
    }

}
