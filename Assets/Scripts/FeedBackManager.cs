using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraPositionMode
{
    Race,
    RaceEnd
}

public class FeedBackManager : MonoBehaviour
{
    private GameObject playerVeichle;
    private VeichleAnchors veichleAnchors;
    [Header("playerVeichle Settings")]
    public float maxTiltAngleZ = 15f;
    public float maxTiltAngleY = 45f;
    public float tiltSmoothSpeed = 5f;

    [Header("Camera Shake Settings")]
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.2f;

    [Header("Camera Settings")]
    public Camera playerCamera;
    public float defaultFov = 60f;
    public float raceBaseFov = 60f;
    public float raceMaxFov = 80f;
    public float turboAdditionalFOV = 10f;

    public CameraPositionMode cameraPositionMode = CameraPositionMode.Race;

    private Coroutine shakeCoroutine;
    private Vector3 originalCamPos;

    private float steeringAmount;

    private float currentTiltAngleZ = 0f;
    private float currentTiltAngleY = 0f;

    private float deltaTime;
    private PlayerController playerController;

    private float currentEnginePower = 0f;
    private bool onTurbo = false;

    // Visual material effect support
    private VeichleVisualEffects veichleVisualEffects;
    private Coroutine visualEffectCoroutine;
    private readonly float visualEffectDurationDefault = 0.5f; // half second as requested

    private List<Material> activeInstanceMaterials = new List<Material>();

    private bool isHuman = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // PlayerController

        playerController = GetComponent<PlayerController>();

        if(playerController == null)
        {
            Debug.LogError("FeedBackManager requires a PlayerController component on the same GameObject.");
            return;
        }

        isHuman = playerController.IsHuman();

        // Player Veichle

        playerVeichle = playerController.GetVeichleModel();

        if(playerVeichle == null)
        {
            Debug.LogError("PlayerController does not have a valid VeichleModel assigned.");
            return;
        }

        steeringAmount = 0;

        // Player Camera

        if (isHuman)
        {
            playerCamera = playerController.GetPlayerCamera();

            if (playerCamera == null)
            {
                Debug.LogError("PlayerController does not have a valid PlayerCamera assigned.");
                return;
            }

            playerCamera.fieldOfView = raceBaseFov;
        }

        

        veichleAnchors = playerController.GetVeichleAnchors();
        cameraPositionMode = CameraPositionMode.Race;

        veichleVisualEffects = playerController.GetVeichlePivot().GetComponent<VeichleVisualEffects>();
        if(veichleVisualEffects == null)
        {
            Debug.LogWarning("VeichleVisualEffects component not found on VeichlePivot. Collision visual effects will be unavailable.");
        }
        else
        {
            InitVisualEffects();
        }
    }

    // Update is called once per frame
    void Update()
    {

        deltaTime = Time.deltaTime;
        if (playerVeichle.activeInHierarchy)
        {
            EngineFeedback();
            SteerFeedBack(steeringAmount);

            if (isHuman)
            {
                ManageCameraFOV();
            }
            
        }

        if(cameraPositionMode == CameraPositionMode.RaceEnd)
        {
            veichleAnchors.cameraPivot.position = veichleAnchors.OutRace_CameraPivot.position;
            veichleAnchors.cameraPivot.rotation = veichleAnchors.OutRace_CameraPivot.rotation;
        }
    }

    private void ManageCameraFOV() {

        float targetFOV = raceBaseFov;

        float lerpTime = 1f;

        if (cameraPositionMode == CameraPositionMode.RaceEnd)
        {
            targetFOV = defaultFov;
        }
        else
        {
            if (onTurbo)
            {
                targetFOV += turboAdditionalFOV;
                lerpTime = 4f;
            }
        }


        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, deltaTime * lerpTime);

    }

    private void InitVisualEffects() { 
        if (veichleVisualEffects != null)
        {
            veichleVisualEffects.SetGlow(0, 0, new Color(0, 0, 0));
        }
    }

    void EngineFeedback()
    {
        if (!onTurbo)
        {
            if (playerController.GetAccelerateInput())
            {
                currentEnginePower = Mathf.Lerp(currentEnginePower, 1f, deltaTime * 2f);
            }
            else
            {
                currentEnginePower = Mathf.Lerp(currentEnginePower, 0f, deltaTime * 2f);
            }
        }
        else
        {
            currentEnginePower = 2f;
        }
        

        playerVeichle.GetComponent<VeichleEffects>().particlePower = 0.5f * currentEnginePower;

    }

    public float GetCurrentEnginePower()
    {
        return currentEnginePower;
    }

    public void SetSteeringFeedBackAmount(float newAmount) { 
        steeringAmount = newAmount;  
    }

    public void OnCollisionFeedback()
    {
        //TriggerCameraShake();

        if (veichleVisualEffects != null)
        {
            if (visualEffectCoroutine != null)
            {
                StopCoroutine(visualEffectCoroutine);
            }
            visualEffectCoroutine = StartCoroutine(veichleVisualEffects.PlayCollisionEffect(0.2f));
        }
    }

    public void OnEnergyRechargeFeedback()
    {
        if (veichleVisualEffects != null)
        {
            if (visualEffectCoroutine != null)
            {
                StopCoroutine(visualEffectCoroutine);
            }
            visualEffectCoroutine = StartCoroutine(veichleVisualEffects.PlayEnergyChargeEffect(0.25f));
        }
    }

    private void TriggerCameraShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);

        }
            

        shakeCoroutine = StartCoroutine(CameraShake());
    }

    public void TurboFeedBack(bool onTurbo) {
        this.onTurbo = onTurbo;

        if (veichleAnchors != null)
        {
            if (onTurbo)
            {
                veichleAnchors.cameraPivot.localPosition = Vector3.Lerp(veichleAnchors.cameraPivot.localPosition, veichleAnchors.Turbo_cameraPivot.localPosition, deltaTime * 4f);
            }
            else
            {
                if((veichleAnchors.cameraPivot.localPosition - veichleAnchors.Normal_cameraPivot.localPosition).magnitude > 0.01f)
                {
                    veichleAnchors.cameraPivot.localPosition = Vector3.Lerp(veichleAnchors.cameraPivot.localPosition, veichleAnchors.Normal_cameraPivot.localPosition, deltaTime);
                }
                else
                {
                    veichleAnchors.cameraPivot.localPosition = veichleAnchors.Normal_cameraPivot.localPosition;
                }
                
            }
        }
    }

    public void TriggerEndRaceCameraView()
    {
        if(veichleAnchors != null)
        {
            cameraPositionMode = CameraPositionMode.RaceEnd;
            
        }
        
    }

    private void SteerFeedBack(float amount)
    {
        float targetTiltAngleZ = -amount * maxTiltAngleZ;
        float targetTiltAngleY = amount * maxTiltAngleY;

        float dt = Time.deltaTime;

        currentTiltAngleZ = Mathf.Lerp(currentTiltAngleZ, targetTiltAngleZ, 1 - Mathf.Exp(-tiltSmoothSpeed * dt));
        currentTiltAngleY = Mathf.Lerp(currentTiltAngleY, targetTiltAngleY, 1 - Mathf.Exp(-tiltSmoothSpeed * dt));

        Vector3 euler = Vector3.zero;
        euler.z = currentTiltAngleZ;
        euler.y = currentTiltAngleY;

        playerVeichle.transform.localRotation = Quaternion.Euler(euler);
    }

    private IEnumerator CameraShake()
    {
        Vector3 originalCamPos = veichleAnchors.cameraPivot.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            // Normalize time (0 .. 1)
            float t = elapsed / shakeDuration;

            // damped spring-like motion
            float damper = Mathf.Exp(-5f * t); // damping
            float spring = Mathf.Sin(t * Mathf.PI * 4f); // oscillation

            // vertical offset: initial hit then bounce
            float offsetY = (-Mathf.Abs(Mathf.Sin(t * Mathf.PI)) + spring) * shakeMagnitude * damper;

            veichleAnchors.cameraPivot.localPosition = originalCamPos + new Vector3(0f, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return exactly to the original position
        veichleAnchors.cameraPivot.localPosition = originalCamPos;
    }
}
