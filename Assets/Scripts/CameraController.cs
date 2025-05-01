using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform cameraTarget;
    public Transform cameraDesiredPosition;

    public float positionSmoothSpeed = 5f;
    public float rotationSmoothSpeed = 5f;

    private float deltaTime = 0;

    void Update()
    {
        deltaTime = Time.deltaTime;

       
    }

    private void LateUpdate()
    {
        if (cameraTarget == null || cameraDesiredPosition == null) return;

        // Posizione smussata con ExpDecay
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = cameraDesiredPosition.position;

        transform.position = new Vector3(
            ExpDecay(currentPosition.x, targetPosition.x, positionSmoothSpeed, deltaTime),
            ExpDecay(currentPosition.y, targetPosition.y, positionSmoothSpeed, deltaTime),
            ExpDecay(currentPosition.z, targetPosition.z, positionSmoothSpeed, deltaTime)
        );

        // Compute target rotation (look at target)
        Quaternion lookRotation = Quaternion.LookRotation(cameraTarget.position - transform.position);

        // Extract tilt (X and Z) from the target rotation
        Vector3 targetEuler = lookRotation.eulerAngles;
        Vector3 targetTilt = cameraTarget.rotation.eulerAngles;

        // Blend Yaw (from lookRotation) and Pitch/Roll (from target)
        Quaternion finalRotation = Quaternion.Euler(
            targetTilt.x,                 // Pitch (inclinazione su X)
            targetEuler.y,                // Yaw (direzione verso target)
            targetTilt.z                  // Roll (inclinazione su Z)
        );

        // Smoothly interpolate to the final rotation
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            finalRotation,
            1 - Mathf.Exp(-rotationSmoothSpeed * deltaTime)
        );
    }

    float ExpDecay(float a, float b, float decay, float deltaTime)
    {
        return Mathf.Lerp(a, b, 1 - Mathf.Exp(-decay * deltaTime));
    }
}
