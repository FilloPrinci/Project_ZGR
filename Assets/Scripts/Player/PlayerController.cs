using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private InputManager inputManager;

    private void Start()
    {
        inputManager = InputManager.Instance;
        if (inputManager == null)
        {
            Debug.LogError("InputManager is not available in the scene. Make sure an InputManager exists.");
            enabled = false; // Blocca l'esecuzione dello script
        }
    }

    private void FixedUpdate()
    {
        ApplyGravityAndHover();
        HandleMovement();
    }

    void HandleMovement()
    {
        if (inputManager.accellerate())
        {
            // accellerate
            Debug.Log("Accellerate");
        }
        else if (inputManager.brake())
        {
            // brake
            Debug.Log("Brake");
        }
        else {
            // brake slowely
        }


        
    }

    void ApplyGravityAndHover()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
        }
    }
}
