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

    private void Start()
    {
        if (cameraDesiredPosition == null)
        {
            Debug.LogError("CameraController: cameraDesiredPosition is not assigned.");
        }
        else
        {
            transform.rotation = cameraDesiredPosition.rotation;
        }
    }

    private void LateUpdate()
    {
        if ( cameraDesiredPosition == null) return;

        Vector3 targetPosition = cameraDesiredPosition.position;

        transform.position = targetPosition;

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
