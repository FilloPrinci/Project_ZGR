using System.Collections.Specialized;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject veichlePrefab;
    private GameObject veichleModelInstance;

    public float maxSpeed = 300f;
    public float rotationMaxSpeed = 10;
    public float rotationAccelleration = 5;
    public float accelleration = 5f;
    public float bounceForce = 1f;
    public float autoBrakeDecelleration = 1f;
    public float brakeDecelleration = 2.5f;
    public float collisionBounceDecelleration = 5f;

    public Transform veichlePivot;

    private InputManager inputManager;
    private FeedBackManager feedBackManager;

    private Vector3 normalMovementVelocity;
    private Vector3 collisionVelocity;

    private float rotationVelocity = 0;
    private float currentRotationSpeed = 0;
    private float deltaTime;

    private bool collisionDetected = false;
    private Vector3 lastCollisionDirection = Vector3.zero;

    // Visual interpolation values
    private Vector3 previousPosition;
    private Vector3 currentPosition;
    private Quaternion previousRotation;
    private Quaternion currentRotation;


    private void Start()
    {
        veichleModelInstance = Instantiate(veichlePrefab, veichlePivot);
        feedBackManager = GetComponent<FeedBackManager>();
        inputManager = InputManager.Instance;
        if (inputManager == null)
        {
            Debug.LogError("InputManager is not available in the scene. Make sure an InputManager exists.");
            enabled = false;
            return;


        }

        // Initialize visual interpolation positions
        currentPosition = transform.position;
        previousPosition = currentPosition;
        currentRotation = transform.rotation;
        previousRotation = currentRotation;

        

        veichlePivot.position = transform.position;
        veichlePivot.rotation = transform.rotation;
    }

    private void Update()
    {
        feedBackManager.SetSteeringFeedBackAmount(inputManager.steer());
    }

    private void LateUpdate()
    {
        // Calculate interpolation factor based on physics update lag
        float interpolationFactor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;

        // Smoothly interpolate the visual model between last and current position/rotation
        veichlePivot.position = Vector3.Lerp(previousPosition, currentPosition, interpolationFactor);
        veichlePivot.rotation = Quaternion.Slerp(previousRotation, currentRotation, interpolationFactor);
    }

    private void FixedUpdate()
    {
        deltaTime = Time.fixedDeltaTime;

        HandleSteering();

        if (!collisionDetected)
        {
            if (collisionVelocity != Vector3.zero)
            {
                Bounce();
            }
            HandleMovement();
        }
        else
        {
            Collide();
        }

        transform.position += (normalMovementVelocity + collisionVelocity);

        ApplyGravityAndHover();

        // Store previous and current transforms for interpolation
        previousPosition = currentPosition;
        previousRotation = currentRotation;
        currentPosition = transform.position;
        currentRotation = transform.rotation;

        if (lastCollisionDirection != Vector3.zero)
        {
            Debug.DrawRay(transform.position, lastCollisionDirection * 3f, Color.red, 0, false);
        }
    }

    void Bounce()
    {
        float currentCollisionSpeed = Speed(collisionVelocity);
        collisionVelocity = collisionVelocity.normalized * AccellerateSpeed(0, collisionBounceDecelleration, currentCollisionSpeed) * deltaTime;
    }

    void Collide()
    {
        normalMovementVelocity = normalMovementVelocity / 2;

        if (lastCollisionDirection != Vector3.zero)
        {
            float currentSpeed = Speed(normalMovementVelocity);
            float finalBounceForce = Mathf.Max(currentSpeed, bounceForce);

            collisionVelocity = lastCollisionDirection.normalized * finalBounceForce * deltaTime;
        }

        feedBackManager.TriggerCameraShake();
    }

    void HandleSteering()
    {
        float steerInput = inputManager.steer();

        if (steerInput != 0)
        {
            rotationVelocity = AccellerateRotationSpeed(rotationMaxSpeed * steerInput, rotationAccelleration);
        }
        else
        {
            rotationVelocity = AccellerateRotationSpeed(0, rotationAccelleration);
        }

        transform.Rotate(0, rotationVelocity * deltaTime, 0, Space.Self);
    }

    void HandleMovement()
    {
        float currentSpeed = Speed(normalMovementVelocity);

        if (inputManager.accellerate())
        {
            normalMovementVelocity = transform.forward * AccellerateSpeed(maxSpeed, accelleration, currentSpeed) * deltaTime;
        }
        else if (inputManager.brake())
        {
            normalMovementVelocity = transform.forward * AccellerateSpeed(0, brakeDecelleration, currentSpeed) * deltaTime;
        }
        else
        {
            normalMovementVelocity = transform.forward * AccellerateSpeed(0, autoBrakeDecelleration, currentSpeed) * deltaTime;
        }
    }

    float AccellerateRotationSpeed(float targetSpeed, float accelleration)
    {
        if (Mathf.Abs(targetSpeed - currentRotationSpeed) < 0.1f)
        {
            currentRotationSpeed = targetSpeed;
        }
        else
        {
            currentRotationSpeed = ExpDecay(currentRotationSpeed, targetSpeed, accelleration, deltaTime);
        }
        return currentRotationSpeed;
    }

    float AccellerateSpeed(float targetSpeed, float accelleration, float currentSpeed)
    {
        if (Mathf.Abs(targetSpeed - currentSpeed) < 0.1f)
        {
            currentSpeed = targetSpeed;
        }
        else
        {
            currentSpeed = ExpDecay(currentSpeed, targetSpeed, accelleration, deltaTime);
        }
        return currentSpeed;
    }

    float ExpDecay(float a, float b, float decay, float deltaTime)
    {
        return Mathf.Lerp(a, b, 1 - Mathf.Exp(-decay * deltaTime));
    }

    void ApplyGravityAndHover()
    {
        float hoverHeight = 0.75f;
        float gravityFallbackSpeed = 50f;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, hoverHeight * 2f))
        {
            Vector3 desiredPosition = hit.point + hit.normal * hoverHeight;
            transform.position = desiredPosition;

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = targetRotation;
        }
        else
        {
            transform.position += Vector3.down * gravityFallbackSpeed * deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Object {other.name} entered trigger.");
        collisionDetected = true;
        lastCollisionDirection = calculateCollisionDirection(other);
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log($"Object {other.name} stays in trigger.");
        collisionDetected = true;
        lastCollisionDirection = calculateCollisionDirection(other);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Object {other.name} exited trigger.");
        collisionDetected = false;
        lastCollisionDirection = Vector3.zero;
    }

    private Vector3 calculateCollisionDirection(Collider otherCollider)
    {
        if (otherCollider is BoxCollider || otherCollider is SphereCollider || otherCollider is CapsuleCollider ||
            (otherCollider is MeshCollider meshCol && meshCol.convex))
        {
            Vector3 contactPoint = otherCollider.ClosestPoint(transform.position);
            return (transform.position - contactPoint).normalized;
        }
        else
        {
            Vector3 direction;
            float distance;

            bool isOverlapping = Physics.ComputePenetration(
                GetComponent<Collider>(), transform.position, transform.rotation,
                otherCollider, otherCollider.transform.position, otherCollider.transform.rotation,
                out direction, out distance
            );

            if (isOverlapping)
            {
                Debug.Log($"[Penetration] Exit direction: {direction}, Distance: {distance}");
                return direction;
            }
            else
            {
                Vector3 fallbackContact = otherCollider.bounds.ClosestPoint(transform.position);
                Debug.LogWarning($"[Fallback] Collider {otherCollider.name} does not support ClosestPoint. Used bounding box.");
                return (transform.position - fallbackContact).normalized;
            }
        }
    }

    float Speed(Vector3 vector)
    {
        return vector.magnitude / deltaTime;
    }

    public GameObject GetVeichleModel()
    {
        return veichleModelInstance;
    }
}
