
using System.Collections;
using UnityEngine;

public class FeedBackManager : MonoBehaviour
{
    private GameObject playerVeichle;
    [Header("playerVeichle Settings")]
    public float maxTiltAngle = 15f;
    public float tiltSmoothSpeed = 5f;
    public GameObject playerCamera;

    [Header("Camera Shake Settings")]
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.2f;

    private Coroutine shakeCoroutine;
    private Vector3 originalCamPos;

    private float steeringAmount;
    private float currentTiltAngle = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerVeichle = GetComponent<PlayerController>().GetVeichleModel();
        steeringAmount = 0;
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

    private void SteerFeedBack(float amount)
    {
        // Calculate the desired Z tilt angle based on input
        float targetTiltAngle = -amount * maxTiltAngle;

        // Smoothly interpolate the current tilt angle using exponential decay
        currentTiltAngle = Mathf.Lerp(currentTiltAngle, targetTiltAngle, 1 - Mathf.Exp(-tiltSmoothSpeed * Time.deltaTime));

        // Apply the tilt to the player's vehicle (Z rotation), preserving X and Y
        Quaternion originalRotation = playerVeichle.transform.parent.rotation;
        Vector3 euler = Vector3.zero;
        euler.z = currentTiltAngle;

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
