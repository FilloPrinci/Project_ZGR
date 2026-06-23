using UnityEngine;

public class FloatInTrack : MonoBehaviour
{
    public float hoverHeight;
    public LayerMask hoverRaycastMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float time = Time.deltaTime;

        ApplyGravityAndHover(time, true);
    }

    private void ApplyGravityAndHover(float time, bool drawLine)
    {
        float gravityFallbackSpeed = 10f;
        Vector3 rayOrigin = transform.position + transform.up;


        RaycastHit hit;
        // Use the layer mask to filter the raycast
        if (drawLine)
        {
            Debug.DrawLine(rayOrigin, rayOrigin - transform.up * hoverHeight * 4f, Color.blue, 0f, false);
        }

        if (Physics.Raycast(rayOrigin, -transform.up, out hit, hoverHeight * 4f, hoverRaycastMask))
        {
            if (drawLine)
            {
                Debug.DrawLine(rayOrigin, hit.point, Color.cyan, 0f, false);
            }

            Vector3 desiredPosition = hit.point + hit.normal * hoverHeight;
            transform.position = desiredPosition;

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            transform.rotation = targetRotation;
        }
        else
        {
            // Fall along local down (relative to current vehicle tilt, not world gravity).
            transform.position -= transform.up * gravityFallbackSpeed * time;
        }
    }
}
