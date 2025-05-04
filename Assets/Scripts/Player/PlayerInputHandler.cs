using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private Vector2 moveInput;
    private bool isAccelerating;

    public float SteerInput => moveInput.x;
    public bool AccelerateInput => isAccelerating;

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
}
