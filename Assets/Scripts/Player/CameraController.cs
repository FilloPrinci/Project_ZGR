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

        // Rotazione smussata con ExpDecay
        Quaternion targetRotation = Quaternion.LookRotation(cameraTarget.position - transform.position);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            1 - Mathf.Exp(-rotationSmoothSpeed * deltaTime)
        );
    }

    float ExpDecay(float a, float b, float decay, float deltaTime)
    {
        return Mathf.Lerp(a, b, 1 - Mathf.Exp(-decay * deltaTime));
    }
}
