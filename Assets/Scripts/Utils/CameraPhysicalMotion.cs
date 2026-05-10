using UnityEngine;

public class CameraPhysicalMotion : MonoBehaviour
{
    [Header("Position Bob")]
    public float moveAmount = 0.03f;
    public float moveSpeed = 1.5f;

    [Header("Rotation Sway")]
    public float rotationAmount = 1.5f;
    public float rotationSpeed = 2f;

    [Header("Noise")]
    public float noiseSpeed = 1f;

    private Vector3 startLocalPos;
    private Quaternion startLocalRot;

    void Start()
    {
        startLocalPos = transform.localPosition;
        startLocalRot = transform.localRotation;
    }

    void Update()
    {
        float time = Time.time * noiseSpeed;

        float offsetX = (Mathf.PerlinNoise(time, 0f) - 0.5f) * 2f;
        float offsetY = (Mathf.PerlinNoise(0f, time) - 0.5f) * 2f;

        Vector3 targetPos = startLocalPos + new Vector3(offsetX, offsetY, 0f) * moveAmount;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPos,
            Time.deltaTime * moveSpeed
        );

        float rotX = (Mathf.PerlinNoise(time, 1f) - 0.5f) * 2f;
        float rotY = (Mathf.PerlinNoise(1f, time) - 0.5f) * 2f;

        Quaternion targetRot = startLocalRot * Quaternion.Euler(
            rotX * rotationAmount,
            rotY * rotationAmount,
            0f
        );

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRot,
            Time.deltaTime * rotationSpeed
        );
    }
}