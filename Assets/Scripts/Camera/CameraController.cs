using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform cameraDesiredPosition;

    public float positionSmoothSpeed = 60f;
    public float rotationSmoothSpeed = 20f;

    [Header("Countdown Camera")]
    public Vector3 countdownPositionOffset = new Vector3(0f, -0.5f, 0.2f);
    public float countdownPitchAngle = 3f;
    public float raceTransitionDuration = 8f;

    private float deltaTime = 0;

    private bool inCountdownMode = false;
    private bool inTransition = false;
    private float transitionElapsed = 0f;
    private Vector3 transitionCurrentOffset = Vector3.zero;

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

    public void EnterCountdownCameraMode()
    {
        inCountdownMode = true;
        inTransition = false;
    }

    public void ExitCountdownCameraMode()
    {
        inCountdownMode = false;
        inTransition = true;
        transitionElapsed = 0f;
        transitionCurrentOffset = cameraDesiredPosition.TransformDirection(countdownPositionOffset);
    }

    private void LateUpdate()
    {
        if (cameraDesiredPosition == null) return;

        if (inCountdownMode)
        {
            float yaw = cameraDesiredPosition.rotation.eulerAngles.y;
            transform.SetPositionAndRotation(
                cameraDesiredPosition.position + cameraDesiredPosition.TransformDirection(countdownPositionOffset),
                Quaternion.Euler(countdownPitchAngle, yaw, 0f));
        }
        else if (inTransition)
        {
            transitionElapsed += deltaTime;
            float t = Mathf.Clamp01(transitionElapsed / raceTransitionDuration);
            float smooth = t * t * (3f - 2f * t);

            float posSpeed = Mathf.Lerp(1f, 10f, smooth);
            float rotSpeed = Mathf.Lerp(1f, rotationSmoothSpeed, smooth);

            transitionCurrentOffset = Vector3.Lerp(transitionCurrentOffset, Vector3.zero, 1f - Mathf.Exp(-posSpeed * deltaTime));
            transform.SetPositionAndRotation(
                cameraDesiredPosition.position + transitionCurrentOffset,
                Quaternion.Slerp(transform.rotation, cameraDesiredPosition.rotation, 1f - Mathf.Exp(-rotSpeed * deltaTime)));

            if (t >= 1f)
            {
                inTransition = false;
                transitionCurrentOffset = Vector3.zero;
            }
        }
        else
        {
            transform.position = cameraDesiredPosition.position;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                cameraDesiredPosition.rotation,
                1 - Mathf.Exp(-rotationSmoothSpeed * deltaTime)
            );
        }
    }
}
