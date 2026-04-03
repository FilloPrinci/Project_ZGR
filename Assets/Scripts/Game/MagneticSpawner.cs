using UnityEngine;

public class MagneticSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject prefab;
    public int count = 10;
    public float stepDistance = 2f;
    public float rayDistance = 5f;

    [Header("Curvature")]
    [Tooltip("Degrees per step (horizontal curve)")]
    public float curvatureAngle = 0f;

    [ContextMenu("Spawn")]
    public void SpawnChain()
    {
        if (prefab == null) return;

        Vector3 currentPosition = transform.position;
        Vector3 currentForward = transform.forward;
        Vector3 currentUp = transform.up;

        GameObject previous = null;

        for (int i = 0; i < count; i++)
        {
            // 1. Applica curvatura (yaw attorno all'up corrente)
            if (Mathf.Abs(curvatureAngle) > 0.001f)
            {
                Quaternion curvatureRotation = Quaternion.AngleAxis(curvatureAngle, currentUp);
                currentForward = curvatureRotation * currentForward;
            }

            // 2. Avanza
            currentPosition += currentForward * stepDistance + currentUp;

            RaycastHit hit;

            // 3. Raycast verso il basso locale
            if (Physics.Raycast(currentPosition, -currentUp, out hit, rayDistance))
            {
                currentPosition = hit.point;

                // 4. Aggiorna up dalla normale
                currentUp = hit.normal;

                // 5. Calcola forward proiettato sul piano
                Vector3 projectedForward = Vector3.ProjectOnPlane(currentForward, currentUp);

                if (projectedForward.sqrMagnitude < 0.001f)
                {
                    projectedForward = Vector3.Cross(currentUp, transform.right);
                }

                Quaternion rotation = Quaternion.LookRotation(projectedForward, currentUp);

                // 6. Instanzia prefab
                GameObject obj = Instantiate(prefab, currentPosition, rotation);

                // 7. Fai guardare il precedente verso questo
                if (previous != null)
                {
                    Vector3 dir = (obj.transform.position - previous.transform.position).normalized;
                    Vector3 projectedDir = Vector3.ProjectOnPlane(dir, previous.transform.up);

                    if (projectedDir.sqrMagnitude > 0.001f)
                    {
                        previous.transform.rotation = Quaternion.LookRotation(projectedDir, previous.transform.up);
                    }
                }

                previous = obj;

                // 8. Aggiorna forward coerente
                currentForward = rotation * Vector3.forward;
            }
            else
            {
                // Se non colpisce nulla, esci (evita spawn nel vuoto)
                Debug.LogWarning("Raycast missed, stopping spawn.");
                break;
            }
        }
    }
}