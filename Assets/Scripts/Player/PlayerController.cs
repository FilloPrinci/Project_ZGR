using NUnit.Framework.Interfaces;
using System;
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
    public PlayerSoundManager playerSoundManager;

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
    private Vector3 playerCollisionDirection = Vector3.zero;

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
    private PlayerCollisionInfo playerCollisionInfo;
    private PlayerCollisionInfo trackCollisionInfo;

    private Vector3 exitVector = Vector3.zero;
    private Vector3 localBounceVector = Vector3.zero;
    private Vector3 localExitVector = Vector3.zero;

    #region Unity Standard Methods

    private void OnValidate()
    {
        if (playerSoundManager == null)
        {
            Debug.LogWarning("PlayerSoundManager reference is missing in PlayerController. Attempting to find it on the same GameObject.");
        }
        else
        {

        }
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
            if (playerData.playerInputIndex != InputIndex.CPU && !autoDrive)
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

        if (raceManager != null)
        {
            trackMainCollider = raceManager.trackMainCollider;
        }

        selfCollider = GetComponent<BoxCollider>();
        if (selfCollider != null)
        {
            selfColliderStartSize = selfCollider.size;
        }

        /*if (speedCoroutine == null)
        {
            speedCoroutine = StartCoroutine(UpdatePositionsForSpeed(5));
        }*/

        playerSoundManager.SetVeichleSoundEffects(playerStructure.GetSoundEffects());
        playerSoundManager.humanPlayer = IsHuman();

    }

    private void Update()
    {
        if (raceManager.IsPaused())
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
            if (collisionDetected)
            {
                if (lastCollisionType == CollisionTypeEnum.Normal)
                {
                    // reset collision flag
                    collisionDetected = false;
                }
            }



            if (trackCollisionInfo != null && trackCollisionInfo.isColliding)
            {
                collisionDetected = true;
                lastCollisionType = CollisionTypeEnum.Normal;
                // calculate exit vector from collision info
                exitVector = trackCollisionInfo.collisionNormal * trackCollisionInfo.penetrationDepth;
                //exitVector.y = 0; // ignore vertical component for bounce

            }
            else if (playerCollisionInfo != null && playerCollisionInfo.isColliding)
            {
                collisionDetected = true;
                lastCollisionType = CollisionTypeEnum.Player;

                // calculate exit vector from collision info
                exitVector = playerCollisionInfo.collisionNormal * playerCollisionInfo.penetrationDepth;
            }
            else
            {
                collisionDetected = false;
                lastCollisionType = CollisionTypeEnum.None;
            }

            if (collisionDetected)
            {
                playerSoundManager.PlayCollisionEffect();

                // manage collision
                globalBounceVector = OnUpadteCollisionDetected(exitVector, deltaTime);
                //globalUpdateMovementVector = globalBounceVector;
                Debug.DrawLine(transform.position, transform.position + exitVector, Color.red, 0, false);

                // bounce vector in local coordinates
                localBounceVector = transform.InverseTransformDirection(globalBounceVector);
                localExitVector = transform.InverseTransformDirection(exitVector);

                // block player from entering the wall by moving it out of the collision
                //transform.position += exitVector;

            }
            else if (localBounceVector.magnitude > 0)
            {
                localExitVector = Vector3.zero;

                if (localBounceVector.magnitude > 0.05)
                {
                    localBounceVector = Utils.ExpDecay(localBounceVector, Vector3.zero, collisionBounceDecelleration, deltaTime);
                }
                else
                {
                    localBounceVector = Vector3.zero;
                }
            }

            ApplyVectorsToLocalPosition(globalUpdateMovementVector, localBounceVector, localExitVector);

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
        veichlePivot.position = currentPosition;

    }

    #endregion

    #region Unity Physix

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Checkpoint"))
        {
            // Checkpoint reached
            raceManager.OnCheckpoint(playerData.name, other.gameObject);
        }
        else if (other.tag.Equals("Item"))
        {
            ItemData itemData = other.GetComponent<ItemData>();
            if (playerStats != null && itemData != null)
            {
                if (itemData.Type == ItemType.Turbo)
                {
                    playerStats.StartTurbo();
                }
                else
                {
                    playerStats.OnItemAcquired(itemData.Type);
                    if (playerStructure != null)
                    {
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
        if (other.tag.Equals("Zone"))
        {
            ZoneData zoneData = other.GetComponent<ZoneData>();

            if (zoneData != null)
            {
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

    #endregion

    #region public attributes methods

    public BoxCollider GetCollider()
    {
        if(selfCollider == null)
        {
            selfCollider = GetComponent<BoxCollider>();
        }

        return selfCollider;
    }

    public void SetPlayerCollisionInfo(PlayerCollisionInfo playerCollisionInfo)
    {
        this.playerCollisionInfo = playerCollisionInfo;
    }

    public void SetTrackCollisionInfo(PlayerCollisionInfo trackCollisionInfo)
    {
        this.trackCollisionInfo = trackCollisionInfo;
    }

    public void ClearPlayerCollisionInfo()
    {
        playerCollisionInfo = null;
    }

    #endregion

    #region public methods

    public bool IsHuman()
    {
        return playerData.playerInputIndex != InputIndex.CPU;
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

        //update camera
        feedBackManager.TriggerEndRaceCameraView();
    }

    public bool GetAccelerateInput()
    {
        return accelerateInput;
    }

    #endregion

    #region private methods

    private void ApplyVectorsToLocalPosition(Vector3 localMovement, Vector3 localBounce, Vector3 localExitVector)
    {
        localBounce.y = 0; // Ensure no vertical movement is applied from bounce vector
        localExitVector.y = 0; // Ensure no vertical movement is applied from exit vector

        Vector3 rotatedLocalBounce = transform.rotation * localBounce;
        Vector3 rotatedLocalExitVector = transform.rotation * localExitVector;


        Vector3 totalLocalMovement = localMovement + rotatedLocalBounce + rotatedLocalExitVector;

        transform.localPosition += totalLocalMovement;
    }

    private void InterpolateVeichlePivotRotation()
    {

        // Smooth rotation using exponential decay on Quaternions
        Quaternion currentRotationQuat = veichlePivot.rotation;
        Quaternion targetRotationQuat = currentRotation;

        float t = 1f - Mathf.Exp(-pivotRotationDecay * deltaTime);
        Quaternion smoothedRotation = Quaternion.Slerp(currentRotationQuat, targetRotationQuat, t);

        veichlePivot.rotation = smoothedRotation;
    }

    private Vector3 OnUpadteCollisionDetected(Vector3 collisionExitDirection, float time)
    {
        float collistionMovementFactor = 1f;
        float collistionBounceFactor = 1f;
        float damageFactor = 1f;

        Vector3 bounceVector = Vector3.zero;

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

        if (collisionExitDirection != Vector3.zero)
        {
            float currentSpeed = Speed(globalUpdateMovementVector, time);
            float finalBounceForce = Mathf.Max(currentSpeed * collistionBounceFactor * bounceFactor, 1f);

            bounceVector = collisionExitDirection.normalized * finalBounceForce * time;
        }

        globalUpdateMovementVector *= collistionMovementFactor;

        feedBackManager.TriggerCameraShake();

        return bounceVector;
    }

    private void HandleSteering(float time)
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

    private Vector3 CalculateLocalMovement(float time)
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

    private float AccellerateRotationSpeed(float targetSpeed, float accelleration, float time)
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

    private float AccellerateSpeed(float targetSpeed, float accelleration, float currentSpeed, float time)
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

    private void StartHoverEngine(float power, float time, bool drawLine)
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

    private void ApplyGravityAndHover(float time, bool drawLine)
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

    private float Speed(Vector3 vector, float time)
    {
        return vector.magnitude / time;
    }

    #endregion
}
