
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
    public GameObject playerCamera;

    [Header("Camera Shake Settings")]
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.2f;

    private Coroutine shakeCoroutine;
    private Vector3 originalCamPos;

    private float steeringAmount;

    private float currentTiltAngleZ = 0f;
    private float currentTiltAngleY = 0f;

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
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(CameraShake());
    }

    public void TurboFeedBack(bool onTurbo) {
        if(veichleAnchors != null)
        {
            if (onTurbo)
            {
                veichleAnchors.cameraPivot.localPosition = Vector3.Lerp(veichleAnchors.cameraPivot.localPosition, veichleAnchors.Turbo_cameraPivot.localPosition, 0.1f);
            }
            else
            {
                veichleAnchors.cameraPivot.localPosition = Vector3.Lerp(veichleAnchors.cameraPivot.localPosition, veichleAnchors.Normal_cameraPivot.localPosition, 0.02f);
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
        yield return null;
        //originalCamPos = playerCamera.transform.localPosition;
        //float elapsed = 0f;
        //
        //while (elapsed < shakeDuration)
        //{
        //    float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
        //    float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;
        //
        //    playerCamera.transform.localPosition = originalCamPos + new Vector3(offsetX, offsetY, 0f);
        //
        //    yield return new WaitForSeconds(0.1f);
        //    elapsed += Time.deltaTime;
        //}
        //
        //playerCamera.transform.localPosition = originalCamPos;
    }
}
