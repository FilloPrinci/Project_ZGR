using UnityEngine;

[ExecuteAlways]
public class MagneticOnStart : MonoBehaviour
{
    public float checkDistance = 5f;
    public bool rotaionAlignment = true;

    [ContextMenu("Align")]
    public void Align()
    {
        if (!gameObject.scene.isLoaded) return;

        RaycastHit hit;

        if (Physics.Raycast(transform.position, -transform.up, out hit, checkDistance))
        {
            transform.position = hit.point;

            if (rotaionAlignment)
            {
                Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, hit.normal);

                if (projectedForward.sqrMagnitude < 0.001f)
                {
                    projectedForward = Vector3.Cross(hit.normal, transform.right);
                }

                transform.rotation = Quaternion.LookRotation(projectedForward, hit.normal);
            }
        }
    }
}