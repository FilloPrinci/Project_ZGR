using UnityEngine;

public class RotateCameraPoint : MonoBehaviour
{
    public float rotationSpeed;
    public float distanceFromCenter;
    public float cameraAngle;
    public float heightOffset;
    public Transform cameraPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cameraPoint.localPosition = new Vector3(0, heightOffset, -distanceFromCenter);   
        cameraPoint.localEulerAngles = new Vector3(cameraAngle, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        float deltaTime = Time.deltaTime;
        transform.Rotate(transform.up, rotationSpeed * deltaTime, Space.Self);
    }
}
