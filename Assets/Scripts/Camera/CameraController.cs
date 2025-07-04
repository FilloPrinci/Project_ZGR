using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform cameraDesiredPosition;

    public float positionSmoothSpeed = 60f;
    public float rotationSmoothSpeed = 20f;

    private float deltaTime = 0;

    void Update()
    {
        deltaTime = Time.deltaTime;

       
    }

    private void LateUpdate()
    {
        if ( cameraDesiredPosition == null) return;

        // Posizione smussata con ExpDecay
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = cameraDesiredPosition.position;

        transform.position = targetPosition;

        //transform.position = new Vector3(
        //    ExpDecay(currentPosition.x, targetPosition.x, positionSmoothSpeed, deltaTime),
        //    ExpDecay(currentPosition.y, targetPosition.y, positionSmoothSpeed, deltaTime),
        //    ExpDecay(currentPosition.z, targetPosition.z, positionSmoothSpeed, deltaTime)
        //);

        // Smoothly interpolate to the final rotation
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            cameraDesiredPosition.rotation,
            1 - Mathf.Exp(-rotationSmoothSpeed * deltaTime)
        );
    }

    float ExpDecay(float a, float b, float decay, float deltaTime)
    {
        return Mathf.Lerp(a, b, 1 - Mathf.Exp(-decay * deltaTime));
    }
}
