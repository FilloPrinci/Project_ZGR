using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Specialized;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

enum CollisionTypeEnum
{
    None,
    Normal,
    Player,
    Obstacle
}

public class PlayerController : MonoBehaviour
{
    public PlayerData playerData;
    private GameObject veichleModelInstance;
    public PlayerStats playerStats;
    public PlayerStructure playerStructure;

    public float hoverHeight = 0.5f;
    public float startHoverHeight = 0.3f;

    public float maxSpeed = 300f;
    public float rotationMaxSpeed = 10;
    public float rotationAccelleration = 5;
    public float accelleration = 5f;
    public float bounceFactor = 0.5f;
    public float autoBrakeDecelleration = 1f;
    public float brakeDecelleration = 2.5f;
    public float collisionBounceDecelleration = 5f;
    public LayerMask hoverRaycastMask;

    public Transform veichlePivot;

    [SerializeField]
    public float pivotRotationDecay = 10f;
    [SerializeField]
    public  float pivotPositionDecay = 10f;

    [SerializeField]
    public bool debugMode = false;

    private GlobalInputManager globalInputManager;
    private PlayerInputHandler playerInputHandler;
    private RaceManager raceManager;
    private FeedBackManager feedBackManager;

    private Vector3 normalMovementVelocity;
    private Vector3 collisionVelocity;

    private float rotationVelocity = 0;
    private float currentRotationSpeed = 0;
    private float fixedDeltaTime;
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

    private CollisionTypeEnum lastCollisionType = CollisionTypeEnum.None;

    private bool autoDrive = false;
    private float enginePower =0f;
    private Collider trackMainCollider;
    private BoxCollider selfCollider;
    private Vector3 globalUpdateMovementVector = Vector3.zero;
    private Vector3 globalBounceVector = Vector3.zero;
    private float globalUpdateSpeed = 0f;

    private Vector3 coroutineCurrentPosition = Vector3.zero;
    private Vector3 coroutineLastPosition = Vector3.zero;

    private Vector3 selfColliderStartSize = Vector3.zero;

    private bool pauseMode = false;

    public BoxCollider GetCollider()
    {
        if(selfCollider == null)
        {
            selfCollider = GetComponent<BoxCollider>();
        }

        return selfCollider;
    }

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
            if(playerData.playerInputIndex != InputIndex.CPU && !autoDrive)
            {
                playerInputHandler = globalInputManager.GetPlayerInput((int)playerData.playerInputIndex).GetComponent<PlayerInputHandler>();
            }
            else
            {
                Debug.Log($"PlayerController Start for CPU player: {playerData.name} with index {playerData.cpuIndex}");
                playerInputHandler = CPUInputHandlerManager.Instance.GetCPUInput(playerData.cpuIndex);
            }



            // Initialize visual interpolation positions
            currentPosition = transform.position;
            previousPosition = currentPosition;
            currentRotation = transform.rotation;
            previousRotation = currentRotation;

            veichlePivot.position = transform.position;
            veichlePivot.rotation = transform.rotation;
        }

        if(raceManager != null)
        {
            trackMainCollider = raceManager.trackMainCollider;
        }

        selfCollider = GetComponent<BoxCollider>();
        if (selfCollider != null) {
            selfColliderStartSize = selfCollider.size;
        }

        /*if (speedCoroutine == null)
        {
            speedCoroutine = StartCoroutine(UpdatePositionsForSpeed(5));
        }*/
    }

    private void Update()
    {
        if(raceManager.IsPaused())
        {
            pauseMode = true;
            return;
        }

        if (pauseMode)
        {
            pauseMode = false;
            Debug.Log("Resuming PlayerController after pause");
            return;
        }
        
        deltaTime = Time.deltaTime;

        // collect input
        if (raceManager.GetCurrentRacePhase() == RacePhase.CountDown || raceManager.GetCurrentRacePhase() == RacePhase.Race || raceManager.GetCurrentRacePhase() == RacePhase.Results)
        {
            steerInput = playerInputHandler.SteerInput;
            accelerateInput = playerInputHandler.AccelerateInput;
        }
        else
        {
            steerInput = 0;
            accelerateInput = false;
        }

        // update player stats
        if (playerStats != null)
        {
            maxSpeed = playerStats.CurrentMaxSpeed;
            accelleration = playerStats.CurrentAcceleration;
            rotationMaxSpeed = playerStats.CurrentRotationSpeed;
            rotationAccelleration = playerStats.CurrentRotationAcceleration;
        }

        // manage race steps
        if (raceManager.GetCurrentRacePhase() == RacePhase.Presentation)
        {
            // place the veichle
            StartHoverEngine(enginePower, deltaTime, debugMode);
        }
        else if (raceManager.GetCurrentRacePhase() == RacePhase.CountDown)
        {
            // hold still and start engine
            StartHoverEngine(enginePower, deltaTime, debugMode);

            if (accelerateInput)
            {
                if (enginePower < 1f)
                {
                    enginePower = Utils.ExpDecay(enginePower, 1f, 2, deltaTime);
                }
            }
            else
            {
                if (enginePower > 0)
                {
                    enginePower = Utils.ExpDecay(enginePower, 0, 5, deltaTime);
                }
            }

        }
        else if (raceManager.GetCurrentRacePhase() == RacePhase.Race || raceManager.GetCurrentRacePhase() == RacePhase.Results)
        {
            // can race on the track
            HandleSteering(deltaTime);
            globalUpdateMovementVector = CalculateLocalMovement(deltaTime);

            Vector3 updateDifference = currentPosition - previousPosition;
            float forwardAmount = Vector3.Dot(updateDifference, transform.forward);

            forwardAmount *= deltaTime;

            // check if collides
            Vector3 exitVector = CheckFastCollision(forwardAmount, debugMode);
            if (exitVector != Vector3.zero)
            {
                collisionDetected = true;
                lastCollisionType = CollisionTypeEnum.Normal;
                globalBounceVector = OnUpadteCollisionDetected(exitVector, deltaTime);
                globalUpdateMovementVector = globalBounceVector;
                Debug.DrawLine(transform.position, transform.position + exitVector, Color.white, 0, false);
                transform.position += exitVector;
            }
            else
            {
                collisionDetected = false;


                if (globalBounceVector != Vector3.zero)
                {
                    if(Utils.IsValid(globalUpdateMovementVector) && Utils.IsValid(globalBounceVector))
                    {
                        transform.localPosition += globalUpdateMovementVector + globalBounceVector;
                        globalBounceVector = Utils.ExpDecay(globalBounceVector, Vector3.zero, collisionBounceDecelleration, deltaTime);
                    }
                }
                else
                {
                    if (Utils.IsValid(globalUpdateMovementVector)){
                        transform.localPosition += globalUpdateMovementVector;
                    }
                }
            }

            // manage hover and gravity
            ApplyGravityAndHover(deltaTime, debugMode);
        }

        // Store previous and current transforms for interpolation
        previousPosition = currentPosition;
        previousRotation = currentRotation;
        currentPosition = transform.position;
        currentRotation = transform.rotation;

        // manage feedback
        feedBackManager.SetSteeringFeedBackAmount(steerInput);
        feedBackManager.TurboFeedBack(playerStats.onTurbo);

        globalUpdateSpeed = Speed(globalUpdateMovementVector, deltaTime);

    }

    private void LateUpdate()
    {
        InterpolateVeichlePivotRotation();
        //InterpolateVeichlePivotPosition();
        veichlePivot.position = currentPosition;
    }

    private void FixedUpdate()
    {


        //fixedDeltaTime = Time.fixedDeltaTime;
        

        /*
        

        if (playerStats != null)
        {
            maxSpeed = playerStats.CurrentMaxSpeed;
            accelleration = playerStats.CurrentAcceleration;
            rotationMaxSpeed = playerStats.CurrentRotationSpeed;
            rotationAccelleration = playerStats.CurrentRotationAcceleration;
        }

        if(raceManager.GetCurrentRacePhase() == RacePhase.Presentation)
        {
            // place the veichle
            StartHoverEngine(enginePower);
        }
        else if (raceManager.GetCurrentRacePhase() == RacePhase.CountDown)
        {
            // hold still and start engine
            StartHoverEngine(enginePower);

            if (accelerateInput)
            {
                if (enginePower < 1f)
                {
                    enginePower = ExpDecay(enginePower, 1f, 2, fixedDeltaTime);
                }
            }
            else
            {
                if (enginePower > 0.25f)
                {
                    enginePower = ExpDecay(enginePower, 0.25f, 5, fixedDeltaTime);
                }
            }

            
                

        }
        else if (raceManager.GetCurrentRacePhase() == RacePhase.Race || raceManager.GetCurrentRacePhase() == RacePhase.Results)
        {
            // can race on the track
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

            

            if (lastCollisionDirection != Vector3.zero)
            {
                Debug.DrawRay(transform.position, lastCollisionDirection * 3f, Color.red, 0, false);
            }

            ApplyGravityAndHover();
        }

        // Store previous and current transforms for interpolation
        previousPosition = currentPosition;
        previousRotation = currentRotation;
        currentPosition = transform.position;
        currentRotation = transform.rotation;

        */

    }

    Vector3 CheckFastCollision(float forwardAmount, bool debug)
    {
        if (trackMainCollider == null || selfCollider == null)
            return Vector3.zero;

        if(forwardAmount != 0)
        {

            selfCollider.size = new Vector3(selfColliderStartSize.x, selfColliderStartSize.y, selfColliderStartSize.z + forwardAmount);
            selfCollider.center = new Vector3(selfCollider.center.x, selfCollider.center.y, forwardAmount / 2);


            //selfCollider.center = transform.forward * selfColliderLocalPosition.z;
        }

        Vector3 direction;
        float distance;

        bool isOverlapping = Physics.ComputePenetration(
            selfCollider, transform.position, transform.rotation,
            trackMainCollider, trackMainCollider.transform.position, trackMainCollider.transform.rotation,
            out direction, out distance
        );

        if (isOverlapping)
        {
            if (debug)
            {
                // Visualize the exit direction
                Debug.DrawLine(transform.position, transform.position + direction * distance, Color.red, 0f, false);
            }
            

            Vector3 localDir = transform.InverseTransformDirection(direction * distance);
            localDir.y = 0f;
            float len = localDir.magnitude;
            localDir.Normalize();
            localDir *= distance; // original lenght

            // Convert back to world space
            Vector3 worldDirXZ = transform.TransformDirection(localDir);

            return worldDirXZ;
        }
        else
        {
            Debug.DrawRay(transform.position, Vector3.up * 0.3f, Color.green, 0f, false);
            return Vector3.zero;
        }

        
    }

    void InterpolateVeichlePivotRotation()
    {
    
        // Smooth rotation using exponential decay on Quaternions
        Quaternion currentRotationQuat = veichlePivot.rotation;
        Quaternion targetRotationQuat = currentRotation;
    
        float t = 1f - Mathf.Exp(-pivotRotationDecay * deltaTime);
        Quaternion smoothedRotation = Quaternion.Slerp(currentRotationQuat, targetRotationQuat, t);
    
        veichlePivot.rotation = smoothedRotation;
    }

    void InterpolateVeichlePivotPosition()
    {

        // Posizione: mantieni XZ precisi (presa da currentPosition), smussa solo Y in spazio locale del pivot
        Vector3 targetPos = currentPosition;

        // Current pos in locale del pivot
        Vector3 localCurrent = veichlePivot.InverseTransformPoint(veichlePivot.position);
        Vector3 localTarget = veichlePivot.InverseTransformPoint(targetPos);

        // Smoothing esponenziale solo sulla Y
        float posT = 1f - Mathf.Exp(-pivotPositionDecay * deltaTime);
        localCurrent.y = Mathf.Lerp(localCurrent.y, localTarget.y, posT);

        // Mantieni X e Z del target (in world) — per evitare slittamenti, converti XZ direttamente
        // Ricostruisci posizione world partendo dalla XZ target e dalla Y smussata (in locale)
        Vector3 worldXZ = new Vector3(targetPos.x, 0f, targetPos.z);
        Vector3 reconstructed = veichlePivot.TransformPoint(new Vector3(localCurrent.x, localCurrent.y, localCurrent.z));

        // Sostituisci XZ con quelli target per sicurezza (evita drift)
        reconstructed.x = worldXZ.x;
        reconstructed.z = worldXZ.z;

        veichlePivot.position = reconstructed;
    }

    void Bounce(float time)
    {
        float currentCollisionSpeed = Speed(collisionVelocity, time);
        collisionVelocity = collisionVelocity.normalized * AccellerateSpeed(0, collisionBounceDecelleration, currentCollisionSpeed, time) * time;
    }

    Vector3 OnUpadteCollisionDetected(Vector3 collisionExitDirection, float time)
    {
        float collistionMovementFactor = 1f;
        float collistionBounceFactor = 1f;
        float damageFactor = 1f;

        Vector3 collisionVector = Vector3.zero;

        if (playerStats != null)
        {


            if (lastCollisionType == CollisionTypeEnum.Player)
            {
                collistionMovementFactor = playerStats.playerSpeedCollisionFactor;
                damageFactor = playerStats.playerDamageFactor;
                collistionBounceFactor = playerStats.playerBounceCollisionFactor;
            }
            else if (lastCollisionType == CollisionTypeEnum.Obstacle)
            {
                collistionMovementFactor = playerStats.obstacleSpeedCollisionFactor;
                damageFactor = playerStats.obstacleDamageFactor;
                collistionBounceFactor = playerStats.obstacleBounceCollisionFactor;
            }
            else if (lastCollisionType == CollisionTypeEnum.Normal)
            {
                collistionMovementFactor = playerStats.normalSpeedCollistionFactor;
                damageFactor = playerStats.normalDamageFactor;
                collistionBounceFactor = playerStats.normalBounceCollistionFactor;
            }

            // recive damage
            playerStats.OnDamage(damageFactor);
            if (playerStructure != null)
            {
                playerStructure.UpdatePlayerGUI(playerStats);
            }

        }

        globalUpdateMovementVector *= collistionMovementFactor;

        if (collisionExitDirection != Vector3.zero)
        {
            float currentSpeed = Speed(globalUpdateMovementVector, time);
            float finalBounceForce = Mathf.Max(currentSpeed * collistionBounceFactor * bounceFactor, 1f);

            collisionVector = collisionExitDirection.normalized * finalBounceForce * time;
        }

        feedBackManager.TriggerCameraShake();

        return collisionVector;
    }

    void Collide(float time)
    {
        float collistionMovementFactor = 1f;
        float collistionBounceFactor = 1f;
        float damageFactor = 1f;

        if (playerStats != null)
        {
            

            if (lastCollisionType == CollisionTypeEnum.Player)
            {
                collistionMovementFactor = playerStats.playerSpeedCollisionFactor;
                damageFactor = playerStats.playerDamageFactor;
                collistionBounceFactor = playerStats.playerBounceCollisionFactor;
            }
            else if (lastCollisionType == CollisionTypeEnum.Obstacle)
            {
                collistionMovementFactor = playerStats.obstacleSpeedCollisionFactor;
                damageFactor = playerStats.obstacleDamageFactor;
                collistionBounceFactor = playerStats.obstacleBounceCollisionFactor;
            }
            else if (lastCollisionType == CollisionTypeEnum.Normal)
            {
                collistionMovementFactor = playerStats.normalSpeedCollistionFactor;
                damageFactor = playerStats.normalDamageFactor;
                collistionBounceFactor = playerStats.normalBounceCollistionFactor;
            }

            // recive damage
            playerStats.OnDamage(damageFactor);
            if (playerStructure != null)
            {
                playerStructure.UpdatePlayerGUI(playerStats);
            }

        }

        normalMovementVelocity = normalMovementVelocity * collistionMovementFactor;
        

        if (lastCollisionDirection != Vector3.zero)
        {
            float currentSpeed = Speed(normalMovementVelocity, time);
            float finalBounceForce = Mathf.Max(currentSpeed * collistionBounceFactor * bounceFactor, 1f);

            collisionVelocity = lastCollisionDirection.normalized * finalBounceForce * fixedDeltaTime;
        }

        feedBackManager.TriggerCameraShake();
    }

    void HandleSteering(float time)
    {

        if (steerInput != 0)
        {
            rotationVelocity = AccellerateRotationSpeed(rotationMaxSpeed * steerInput, rotationAccelleration, time);
        }
        else
        {
            rotationVelocity = AccellerateRotationSpeed(0, rotationAccelleration, time);
        }

        transform.Rotate(0, rotationVelocity * time, 0, Space.Self);
    }

    Vector3 CalculateLocalMovement(float time)
    {
        Vector3 movement = Vector3.zero;
        float currentSpeed = globalUpdateMovementVector.magnitude / time;

        if (accelerateInput)
        {
            movement = transform.forward * AccellerateSpeed(maxSpeed, accelleration, currentSpeed, time) * time;
        }
        else
        {
            movement = transform.forward * AccellerateSpeed(0, autoBrakeDecelleration, currentSpeed, time) * time;
        }

            return movement;
    }

    void HandleMovement(float time)
    {
        float currentSpeed = Speed(normalMovementVelocity, time);

        if (accelerateInput)
        {
            normalMovementVelocity = transform.forward * AccellerateSpeed(maxSpeed, accelleration, currentSpeed, fixedDeltaTime) * fixedDeltaTime;
        }
        else
        {
            normalMovementVelocity = transform.forward * AccellerateSpeed(0, autoBrakeDecelleration, currentSpeed, fixedDeltaTime) * fixedDeltaTime;
        }
    }

    float AccellerateRotationSpeed(float targetSpeed, float accelleration, float time)
    {
        if (Mathf.Abs(targetSpeed - currentRotationSpeed) < 0.1f)
        {
            currentRotationSpeed = targetSpeed;
        }
        else
        {
            currentRotationSpeed = Utils.ExpDecay(currentRotationSpeed, targetSpeed, accelleration, time);
        }
        return currentRotationSpeed;
    }

    float AccellerateSpeed(float targetSpeed, float accelleration, float currentSpeed, float time)
    {
        if (Mathf.Abs(targetSpeed - currentSpeed) < 0.1f)
        {
            currentSpeed = targetSpeed;
        }
        else
        {
            currentSpeed = Utils.ExpDecay(currentSpeed, targetSpeed, accelleration, time);
        }
        return currentSpeed;
    }


    void StartHoverEngine(float power, float time, bool drawLine)
    {
        float gravityFallbackSpeed = 10f;

        RaycastHit hit;
        Vector3 rayOrigin = transform.position + transform.up;

        if (drawLine)
        {
            Debug.DrawLine(rayOrigin, rayOrigin - transform.up * hoverHeight * 4f, Color.blue, 0f, false);
        }

        // Use the layer mask to filter the raycast
        if (Physics.Raycast(rayOrigin, -transform.up, out hit, hoverHeight * 4f, hoverRaycastMask))
        {
            if (drawLine)
            {
                Debug.DrawLine(rayOrigin, hit.point, Color.cyan, 0f, false);
            }
            Vector3 desiredPosition = hit.point + (hit.normal * startHoverHeight) + (hit.normal * (hoverHeight - startHoverHeight) * power);
            transform.position = desiredPosition;

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = targetRotation;
        }
        else
        {
            transform.position += Vector3.down * gravityFallbackSpeed * time;
        }
    }

    void ApplyGravityAndHover(float time, bool drawLine)
    {
        float gravityFallbackSpeed = 10f;
        Vector3 rayOrigin = transform.position + transform.up;


        RaycastHit hit;
        // Use the layer mask to filter the raycast
        if (drawLine)
        {
            Debug.DrawLine(rayOrigin, rayOrigin - transform.up * hoverHeight * 4f, Color.blue, 0f, false);
        }
        
        if (Physics.Raycast(rayOrigin, -transform.up, out hit, hoverHeight * 4f, hoverRaycastMask))
        {
            if (drawLine)
            {
                Debug.DrawLine(rayOrigin, hit.point, Color.cyan, 0f, false);
            }

            if (enginePower > 0.9f)
            {
                enginePower = 1f;
            }
            else
            {
                enginePower = Utils.ExpDecay(enginePower, 1f, 10, time);
            }
            Vector3 desiredPosition = hit.point + hit.normal * hoverHeight * enginePower;
            transform.position = desiredPosition;

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = targetRotation;
        }
        else
        {
            transform.position += Vector3.down * gravityFallbackSpeed * time;
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

    private bool specialCollision(Collider collider)
    {
        return collider.tag.Equals("Player") || collider.tag.Equals("Obstacle");
    }

    private void OnTriggerEnter(Collider other)
    {
        if(specialCollision(other))
        {
            if(other.tag.Equals("Player"))
            {
                lastCollisionType = CollisionTypeEnum.Player;
            }
            else if (other.tag.Equals("Obstacle"))
            {
                lastCollisionType = CollisionTypeEnum.Obstacle;
            }

            collisionDetected = true;

            //lastCollisionDirection = calculateCollisionDirection(other);
            Vector3 exitVector = calculateCollisionDirection(other);
            globalBounceVector = OnUpadteCollisionDetected(exitVector, deltaTime);
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
        if (specialCollision(other))
        {
            if (other.tag.Equals("Player"))
            {
                lastCollisionType = CollisionTypeEnum.Player;
            }
            else if (other.tag.Equals("Obstacle"))
            {
                lastCollisionType = CollisionTypeEnum.Obstacle;
            }
            else
            {
                lastCollisionType = CollisionTypeEnum.Normal;
            }

            collisionDetected = true;
            lastCollisionDirection = calculateCollisionDirection(other);
        }
        else if (other.tag.Equals("Zone"))
        {
            ZoneData zoneData = other.GetComponent<ZoneData>();

            if (zoneData != null) {
                if (playerStats != null && zoneData != null)
                {
                    if (zoneData.Type == ZoneType.Recharge)
                    {
                        float currentSpeed = GetCurrentSpeed();
                        playerStats.OnEnergyRechargeBySpeed(Time.fixedDeltaTime, currentSpeed);
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
    }

    private void OnTriggerExit(Collider other)
    {
        /*
        if (ShouldHandleCollision(other))
        {
            
            lastCollisionType = CollisionTypeEnum.None;

            collisionDetected = false;
            lastCollisionDirection = Vector3.zero;
        }*/
    }

    public void onOtherPlayerCollisionDetected(Collider other)
    {
        lastCollisionType = CollisionTypeEnum.Player;
        collisionDetected = true;
        lastCollisionDirection = calculateCollisionDirection(other);
    }

    private Vector3 calculateCollisionDirection(Collider otherCollider)
    {
        Vector3 worldDirection;

        if (otherCollider is BoxCollider || otherCollider is SphereCollider || otherCollider is CapsuleCollider ||
            (otherCollider is MeshCollider meshCol && meshCol.convex))
        {
            Vector3 contactPoint = otherCollider.ClosestPoint(transform.position);
            worldDirection = (transform.position - contactPoint).normalized;
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

                worldDirection = direction;
            }
            else
            {
                Vector3 fallbackContact = otherCollider.bounds.ClosestPoint(transform.position);
                worldDirection = (transform.position - fallbackContact).normalized;
            }
        }

        // Convert the direction from world space to local space
        Vector3 localDir = transform.InverseTransformDirection(worldDirection);

        // Ignore the Y axis to keep only the XZ plane
        localDir.y = 0f;

        // Normalize to prevent distortion after removing Y
        localDir.Normalize();

        // Convert back to world space if you want to apply it globally
        return transform.TransformDirection(localDir);
    }

    float Speed(Vector3 vector, float time)
    {
        return vector.magnitude / time;
    }
    public float GetCurrentSpeed()
    {
        return globalUpdateSpeed;
    }

    public float GetMaxSpeed()
    {
        return maxSpeed;
    }

    public GameObject GetVeichleModel()
    {
        return veichleModelInstance;
    }

    public VeichleAnchors GetVeichleAnchors()
    {
        return veichlePivot.gameObject.GetComponent<VeichleAnchors>();
    }

    public void EndRace()
    {
        if (!autoDrive)
        {
            autoDrive = true;
        }

        // update input
        playerInputHandler = CPUInputHandlerManager.Instance.GetCPUInput(playerData.cpuIndex);
    }

    public bool GetAccelerateInput()
    {
        return accelerateInput;
    }

}
