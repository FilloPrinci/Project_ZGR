
using System.Collections;
using UnityEngine;

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

    private Coroutine shakeCoroutine;
    private Vector3 originalCamPos;

    private float steeringAmount;

    private float currentTiltAngleZ = 0f;
    private float currentTiltAngleY = 0f;

    private float deltaTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerVeichle = GetComponent<PlayerController>().GetVeichleModel();
        steeringAmount = 0;

        veichleAnchors = GetComponent<PlayerController>().GetVeichleAnchors();
    }

    // Update is called once per frame
    void Update()
    {
        deltaTime = Time.deltaTime;
        if (playerVeichle.activeInHierarchy)
        {
            SteerFeedBack(steeringAmount);
        }
             
    }

    public void SetSteeringFeedBackAmount(float newAmount) { 
        steeringAmount = newAmount;  
    }

    public void TriggerCameraShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);

        }
            

        shakeCoroutine = StartCoroutine(CameraShake());
    }

    public void TurboFeedBack(bool onTurbo) {
        if(veichleAnchors != null)
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
            // Normalizza il tempo (0 ? 1)
            float t = elapsed / shakeDuration;

            // 1?? Inizio: botta verso il basso
            // 2?? Poi effetto molla (oscillazione smorzata)
            float damper = Mathf.Exp(-5f * t); // smorzamento
            float spring = Mathf.Sin(t * Mathf.PI * 4f); // oscillazione

            // Movimento verticale: inizia con una botta negativa e poi rimbalza
            float offsetY = (-Mathf.Abs(Mathf.Sin(t * Mathf.PI)) + spring) * shakeMagnitude * damper;

            veichleAnchors.cameraPivot.localPosition = originalCamPos + new Vector3(0f, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ritorna esattamente alla posizione originale
        veichleAnchors.cameraPivot.localPosition = originalCamPos;
    }
}
