using NUnit.Framework.Interfaces;
using System.Collections.Specialized;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerData playerData;
    private GameObject veichleModelInstance;
    public PlayerStats playerStats;
    public PlayerStructure playerStructure;

    public float hoverHeight = 0.5f;

    public float maxSpeed = 300f;
    public float rotationMaxSpeed = 10;
    public float rotationAccelleration = 5;
    public float accelleration = 5f;
    public float bounceForce = 1f;
    public float autoBrakeDecelleration = 1f;
    public float brakeDecelleration = 2.5f;
    public float collisionBounceDecelleration = 5f;
    public LayerMask hoverRaycastMask;

    public Transform veichlePivot;

    [SerializeField]
    public float pivotRotationDecay = 10f;
    [SerializeField]
    public  float pivotPositionDecay = 10f;

    private GlobalInputManager globalInputManager;
    private PlayerInputHandler playerInputHandler;
    private RaceManager raceManager;
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

    private float steerInput = 0f;
    private bool accelerateInput = false;


    private void Start()
    {
        veichleModelInstance = Instantiate(playerData.veichlePrefab, veichlePivot);
        feedBackManager = GetComponent<FeedBackManager>();
        globalInputManager = GlobalInputManager.Instance;
        raceManager = RaceManager.Instance;
        
        if (globalInputManager == null)
        {
            Debug.LogError("GlobalInputManager is not available in the scene. Make sure an GlobalInputManager exists.");
            enabled = false;
            return;
        }
        else
        {
            playerInputHandler = globalInputManager.GetPlayerInput((int)playerData.playerInputIndex).GetComponent<PlayerInputHandler>();

            // Initialize visual interpolation positions
            currentPosition = transform.position;
            previousPosition = currentPosition;
            currentRotation = transform.rotation;
            previousRotation = currentRotation;



            veichlePivot.position = transform.position;
            veichlePivot.rotation = transform.rotation;
        }

            
    }

    private void Update()
    {
        if(raceManager.GetCurrentRacePhase() == RacePhase.Race)
        {
            steerInput = playerInputHandler.SteerInput;
            accelerateInput = playerInputHandler.AccelerateInput;
        }
        else
        {
            steerInput = 0;
            accelerateInput = false;
        }

            feedBackManager.SetSteeringFeedBackAmount(steerInput);
    }

    private void LateUpdate()
    {
        InterpolateVeichlePivot();
    }

    private void FixedUpdate()
    {
        if (playerStats != null)
        {
            maxSpeed = playerStats.CurrentMaxSpeed;
            accelleration = playerStats.CurrentAcceleration;
            rotationMaxSpeed = playerStats.CurrentRotationSpeed;
            rotationAccelleration = playerStats.CurrentRotationAcceleration;
        }

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

    void InterpolateVeichlePivot()
    {

        // Smooth rotation using exponential decay on Quaternions
        Quaternion currentRotationQuat = veichlePivot.rotation;
        Quaternion targetRotationQuat = currentRotation;

        float t = 1f - Mathf.Exp(-pivotRotationDecay * deltaTime);
        Quaternion smoothedRotation = Quaternion.Slerp(currentRotationQuat, targetRotationQuat, t);

        veichlePivot.rotation = smoothedRotation;

        // Smooth position using exponential decay
        //veichlePivot.position = ExpDecay(veichlePivot.position, currentPosition, pivotPositionDecay, deltaTime);
        veichlePivot.position = currentPosition;
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

        if (accelerateInput)
        {
            normalMovementVelocity = transform.forward * AccellerateSpeed(maxSpeed, accelleration, currentSpeed) * deltaTime;
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

    Vector3 ExpDecay(Vector3 a, Vector3 b, float decay, float deltaTime)
    {
        return new Vector3(
            ExpDecay(a.x, b.x, decay, deltaTime),
            ExpDecay(a.y, b.y, decay, deltaTime),
            ExpDecay(a.z, b.z, decay, deltaTime)
        );
    }

    void ApplyGravityAndHover()
    {
        float gravityFallbackSpeed = 50f;

        RaycastHit hit;
        // Use the layer mask to filter the raycast
        if (Physics.Raycast(transform.position, -transform.up, out hit, hoverHeight * 2f, hoverRaycastMask))
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

    private bool ShouldHandleCollision(Collider other)
    {
        bool handleCollision = true;

        if (other.tag.Equals("Checkpoint") || other.tag.Equals("Item") || other.tag.Equals("Zone"))
        {
            handleCollision = false;
        }

        return handleCollision;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ShouldHandleCollision(other))
        {
            collisionDetected = true;
            lastCollisionDirection = calculateCollisionDirection(other);
            // recive damage
            if (playerStats != null)
            {
                playerStats.OnDamage();
                if (playerStructure != null)
                {
                    playerStructure.UpdatePlayerGUI(playerStats);
                }
            }
        }
        else if (other.tag.Equals("Checkpoint"))
        {
            // Checkpoint reached
            raceManager.OnCheckpoint(playerData.name, other.gameObject);
        }else if (other.tag.Equals("Item"))
        {
            ItemData itemData = other.GetComponent<ItemData>();
            if (playerStats != null && itemData != null) {
                if (itemData.Type == ItemType.Turbo) {
                    playerStats.StartTurbo();
                }
                else
                {
                    playerStats.OnItemAcquired(itemData.Type);
                    if (playerStructure != null) {
                        playerStructure.UpdatePlayerGUI(playerStats);
                    }

                }
            }
            else
            {
                Debug.LogError("access to item data failed");
            }
        }
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (ShouldHandleCollision(other))
        {
            collisionDetected = true;
            lastCollisionDirection = calculateCollisionDirection(other);
        }
        else if (other.tag.Equals("Zone"))
        {
            ZoneData zoneData = other.GetComponent<ZoneData>();

            if (playerStats != null && zoneData != null)
            {
                if(zoneData.Type == ZoneType.Recharge)
                {
                    float currentSpeed = Speed(normalMovementVelocity);
                    playerStats.OnEnergyRechargeBySpeed(deltaTime, currentSpeed);
                    if (playerStructure != null)
                    {
                        playerStructure.UpdatePlayerGUI(playerStats);
                    }
                }
            }
            else
            {
                Debug.LogError("access to zoneData failed");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ShouldHandleCollision(other))
        {
            collisionDetected = false;
            lastCollisionDirection = Vector3.zero;
        }
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
