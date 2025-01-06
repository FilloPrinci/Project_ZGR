using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 300f;
    public float rotationMaxSpeed = 10;
    public float rotationAccelleration = 5;
    public float accelleration = 5f;

    private InputManager inputManager;

    private Vector3 velocity;
    private float currentSpeed = 0;

    private float rotationVelocity = 0;
    private float currentRotationSpeed = 0;

    private float currentHoverHeight;
    private Vector3 targetPosition;

    private float deltaTime;

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
        deltaTime = Time.deltaTime;

        ApplyGravityAndHover();
        HandleSteering();
        HandleMovement();
        
    }

    void HandleSteering() {
        float steerInput = inputManager.steer();

        if (steerInput != 0)
        {
            // accellerate roation to desired rotation
            rotationVelocity = AccellerateRotationSpeed(rotationMaxSpeed * steerInput, rotationAccelleration);
        }
        else
        {
            // brake rotation
            rotationVelocity = AccellerateRotationSpeed(0, rotationAccelleration);
        }
        

        transform.Rotate(0, rotationVelocity * deltaTime, 0, Space.World);
    }

    void HandleMovement()
    {
        if (inputManager.accellerate())
        {
            // accellerate
            velocity = transform.forward * AccellerateSpeed(maxSpeed, accelleration) * deltaTime;
        }
        else if (inputManager.brake())
        {
            // brake
            velocity = transform.forward * AccellerateSpeed(0, 1) * deltaTime;
        }
        else {
            // brake slowely
            velocity = transform.forward * AccellerateSpeed(0, 1) * deltaTime;
        }

        transform.position += velocity;

    }

    float AccellerateRotationSpeed(float targetSpeed, float accelleration)
    {
        currentRotationSpeed = ExpDecay(currentRotationSpeed, targetSpeed, accelleration, deltaTime);
        if (Mathf.Abs(targetSpeed - currentRotationSpeed) < 0.01)
        {
            currentRotationSpeed = targetSpeed;
        }
        return currentRotationSpeed;
    }

    float AccellerateSpeed(float targetSpeed, float accelleration) {
        currentSpeed = ExpDecay(currentSpeed, targetSpeed, accelleration, deltaTime);
        if (Mathf.Abs(targetSpeed - currentSpeed) < 0.01)
        {
            currentSpeed = targetSpeed;
        }
        return currentSpeed;
    }

    float ExpDecay(float a, float b, float decay, float deltaTime)
    {
        return Mathf.Lerp(a, b, 1 - Mathf.Exp(-decay * deltaTime));
    }

    void ApplyGravityAndHover()
    {
        float hoverHeight = 2.0f;          // Altezza fissa sopra la superficie
        float positionAdjustSpeed = 5f;   // Velocità di adattamento verticale
        float rotationAdjustSpeed = 10f;  // Velocità di adattamento dell'orientamento
        float gravityFallbackSpeed = 10f; // Velocità della gravità quando non tocca terra

        RaycastHit hit;

        // Raycast verso il basso
        if (Physics.Raycast(transform.position, -transform.up, out hit, hoverHeight * 2f))
        {
            // Posizione target: punto d'impatto + altezza desiderata sopra il terreno
            Vector3 desiredPosition = hit.point + hit.normal * hoverHeight;

            // Interpolazione dell'altezza verso la posizione target con ExpDecay
            currentHoverHeight = ExpDecay(currentHoverHeight, desiredPosition.y, positionAdjustSpeed, deltaTime);
            targetPosition = new Vector3(transform.position.x, currentHoverHeight, transform.position.z);

            // Imposta la posizione verticale con stabilità magnetica
            transform.position = targetPosition;

            // Rotazione: allineamento graduale alla normale
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deltaTime * rotationAdjustSpeed);
        }
        else
        {
            // Se non c'è contatto con il terreno, usa la gravità
            transform.position += Vector3.up * gravityFallbackSpeed * deltaTime;
        }
    }
}
