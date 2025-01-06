using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 300f;
    public float accelleration = 5f;

    private InputManager inputManager;
    private Vector3 velocity;
    private float currentSpeed = 0;

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
            velocity = transform.forward * accellerateSpeed(maxSpeed, accelleration) * Time.deltaTime;
        }
        else if (inputManager.brake())
        {
            // brake
            velocity = transform.forward * accellerateSpeed(0, 1) * Time.deltaTime;
        }
        else {
            // brake slowely
            velocity = transform.forward * accellerateSpeed(0, 1) * Time.deltaTime;
        }

        transform.position += velocity;

    }

    float accellerateSpeed(float targetSpeed, float accelleration) {
        currentSpeed = expDecay(currentSpeed, targetSpeed, accelleration, Time.deltaTime);
        if (Mathf.Abs(targetSpeed - currentSpeed) < 0.01)
        {
            currentSpeed = targetSpeed;
        }
        return currentSpeed;
    }

    float expDecay(float a, float b, float decay, float deltaTime)
    {
        return Mathf.Lerp(a, b, 1 - Mathf.Exp(-decay * deltaTime));
    }

    void ApplyGravityAndHover()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
        }
    }
}
