
using UnityEngine;

public class FeedBackManager : MonoBehaviour
{
    public GameObject playerVeichle;
    [Header("playerVeichle Settings")]
    public float maxTiltAngle = 15f;
    public float tiltSmoothSpeed = 5f;
    public GameObject playerCamera;

    private float steeringAmount;
    private float currentTiltAngle = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        steeringAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        SteerFeedBack(steeringAmount);        
    }

    public void SetSteeringFeedBackAmount(float newAmount) { 
        this.steeringAmount = newAmount;  
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
        euler.z  = currentTiltAngle;

        playerVeichle.transform.localRotation = Quaternion.Euler(euler);
    }
}
