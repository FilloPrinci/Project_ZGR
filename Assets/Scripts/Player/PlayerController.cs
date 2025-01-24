using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 300f;
    public float rotationMaxSpeed = 10;
    public float rotationAccelleration = 5;
    public float accelleration = 5f;
    public float bounceForce = 1f;
    public float autoBrakeDecelleration = 1f;
    public float brakeDecelleration = 2.5f;
    public float collisionBounceDecelleration = 5f;

    public GameObject collider;

    private InputManager inputManager;

    private Vector3 normalMovementVelocity;

    private Vector3 collisionVelocity;

    private float rotationVelocity = 0;
    private float currentRotationSpeed = 0;

    private float currentHoverHeight;
    private Vector3 targetPosition;

    private float deltaTime;

    private bool collisionDetected = false;
    private Vector3 lastCollisionDirection = Vector3.zero;

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

        HandleSteering();

        if (!collisionDetected)
        {
            if (collisionVelocity != Vector3.zero) {
                Bounce();
            }
            
            HandleMovement();
        }
        else {

            // collision
            Collide();
        }

        // applyng velocity
        transform.position += (normalMovementVelocity + collisionVelocity);

        ApplyGravityAndHover();

    }

    void Bounce() {
        float curerntCollisionSpeed = Speed(collisionVelocity);

        collisionVelocity = collisionVelocity.normalized * AccellerateSpeed(0, collisionBounceDecelleration, curerntCollisionSpeed) * deltaTime;
    }

    void Collide() {

        normalMovementVelocity = normalMovementVelocity / 2;

        
        if (lastCollisionDirection != Vector3.zero) {
            /*
            Vector3 collisionDirectionNormalized = lastCollisionDirection.normalized;

            float collisionMagnitude = collisionDirectionNormalized.magnitude;
            */

            float currentSpeed = Speed(normalMovementVelocity);

            float finalBounceForce = currentSpeed;

            if (currentSpeed < bounceForce) {
                finalBounceForce = bounceForce;
            }

            

            collisionVelocity = lastCollisionDirection.normalized * finalBounceForce * deltaTime;
        }
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
        float currentSpeed = Speed(normalMovementVelocity);

        if (inputManager.accellerate())
        {
            // accellerate
            normalMovementVelocity = transform.forward * AccellerateSpeed(maxSpeed, accelleration, currentSpeed) * deltaTime;
        }
        else if (inputManager.brake())
        {
            // brake
            normalMovementVelocity = transform.forward * AccellerateSpeed(0, brakeDecelleration, currentSpeed) * deltaTime;
        }
        else {
            // brake slowely
            normalMovementVelocity = transform.forward * AccellerateSpeed(0, autoBrakeDecelleration, currentSpeed) * deltaTime;
        }

    }

    float AccellerateRotationSpeed(float targetSpeed, float accelleration)
    {
        
        if (Mathf.Abs(targetSpeed - currentRotationSpeed) < 0.1)
        {
            currentRotationSpeed = targetSpeed;
        }
        else {
            currentRotationSpeed = ExpDecay(currentRotationSpeed, targetSpeed, accelleration, deltaTime);
        }
        return currentRotationSpeed;
    }

    float AccellerateSpeed(float targetSpeed, float accelleration, float currentSpeed) {
        
        if (Mathf.Abs(targetSpeed - currentSpeed) < 0.1)
        {
            currentSpeed = targetSpeed;
        }
        else {
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
        float hoverHeight = 1.5f;
        
        float gravityFallbackSpeed = 50;

        RaycastHit hit;

        // Raycast 
        if (Physics.Raycast(transform.position, -transform.up, out hit, hoverHeight * 2f))
        {
            // Target position
            Vector3 desiredPosition = hit.point + hit.normal * hoverHeight;

            transform.position = desiredPosition;

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            transform.rotation = targetRotation;

            /// TODO: use this part for camera and model animation only, not for the logic entity
            /*
            // normal based rotation lerped

            float angleThreshold = 1f;
            float rotationAdjustSpeed = 20f;
            float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
            if (angleDifference <= angleThreshold)
            {
                transform.rotation = targetRotation;
            }
            else {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deltaTime * rotationAdjustSpeed);
            }*/


        }
        else
        {
            // Use gravity when no plane is detected
            transform.position += Vector3.down * gravityFallbackSpeed * deltaTime;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Oggetto {other.name} � entrato nel trigger.");
        collisionDetected = true;
        lastCollisionDirection = calculateCollisionDirection(other);
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log($"Oggetto {other.name} � ancora nel trigger.");
        collisionDetected = true;
        lastCollisionDirection = calculateCollisionDirection(other);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Oggetto {other.name} � uscito dal trigger.");
        collisionDetected = false;
        lastCollisionDirection = Vector3.zero;
    }

    private Vector3 calculateCollisionDirection(Collider collider) {
        Vector3 globalDirection = collider.ClosestPoint(transform.position) - transform.position;
        globalDirection = globalDirection * -1;

        return globalDirection;
    }

    float Speed(Vector3 spaceVector) { 
        float speed = spaceVector.magnitude / deltaTime;
        
        return speed;
    }

}
