using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
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

    [Header("Editor")]
    public bool autoUpdate = true;

    private Transform container;

    bool isGenerating = false;

    // --------------------------------------------------

    private void OnEnable()
    {
        if (!gameObject.scene.isLoaded) return;
        Generate();
    }

    private void OnValidate()
    {
        if (!autoUpdate) return;

        #if UNITY_EDITOR
                if (EditorApplication.isCompiling) return;

                // evita chiamate multiple nello stesso frame
                EditorApplication.delayCall -= DelayedGenerate;
                EditorApplication.delayCall += DelayedGenerate;
        #endif
    }

    #if UNITY_EDITOR
    void DelayedGenerate()
    {
        if (this == null) return;
        if (isGenerating) return;

        Generate();
    }
#endif

    // --------------------------------------------------

    [ContextMenu("Generate")]
    [ContextMenu("Generate")]
    public void Generate()
    {
        if (Application.isPlaying) return;

        if (!gameObject.scene.isLoaded) return;

        if (isGenerating) return;
        isGenerating = true;

        if (prefab == null)
        {
            isGenerating = false;
            return;
        }

        EnsureContainer();

#if UNITY_EDITOR
        Undo.RegisterFullObjectHierarchyUndo(container.gameObject, "Regenerate Chain");
#endif

        ClearContainer();

        Vector3 currentPosition = transform.position;
        Vector3 currentForward = transform.forward;
        Vector3 currentUp = transform.up;

        GameObject previous = null;

        for (int i = 0; i < count; i++)
        {
            // Raycast from slightly above current position to find the surface.
            // On the first iteration currentPosition is the spawner position,
            // so the first element is placed directly below it.
            RaycastHit hit;

            if (Physics.Raycast(currentPosition + currentUp, -currentUp, out hit, rayDistance))
            {
                currentPosition = hit.point;
                currentUp = hit.normal;

                Vector3 projectedForward = Vector3.ProjectOnPlane(currentForward, currentUp);

                if (projectedForward.sqrMagnitude < 0.001f)
                {
                    projectedForward = Vector3.Cross(currentUp, transform.right);
                }

                Quaternion rotation = Quaternion.LookRotation(projectedForward, currentUp);

                GameObject obj;

                #if UNITY_EDITOR
                    obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, container);
                    obj.transform.position = currentPosition;
                    obj.transform.rotation = rotation;
                    Undo.RegisterCreatedObjectUndo(obj, "Create Segment");
                #else
                    obj = Instantiate(prefab, currentPosition, rotation, container);
                #endif

                obj.name = prefab.name + "_" + i;

                // orienta il precedente
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
                currentForward = rotation * Vector3.forward;

                // Avanza per la prossima iterazione — dopo aver piazzato l'elemento,
                // così il primo parte dalla posizione dello spawner.
                if (Mathf.Abs(curvatureAngle) > 0.001f)
                {
                    Quaternion curvatureRotation = Quaternion.AngleAxis(curvatureAngle, currentUp);
                    currentForward = curvatureRotation * currentForward;
                }

                currentPosition += currentForward * stepDistance + currentUp;
            }
            else
            {
                break;
            }
        }

        isGenerating = false;
    }

    // --------------------------------------------------

    void EnsureContainer()
    {
        if (container != null) return;

        Transform existing = transform.Find("Generated");

        if (existing != null)
        {
            container = existing;
        }
        else
        {
            GameObject go = new GameObject("Generated");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            container = go.transform;
        }
    }

    void ClearContainer()
    {
        if (container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);

        #if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.DestroyObjectImmediate(child.gameObject);
            else
                Destroy(child.gameObject);
        #else
            Destroy(child.gameObject);
        #endif
        }
    }
}