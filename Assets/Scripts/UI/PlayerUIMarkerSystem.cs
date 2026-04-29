using System.Collections.Generic;
using UnityEngine;

class MarkerData
{
    public RectTransform rect;
    public CanvasGroup canvasGroup;
    public float targetAlpha;
    public bool isBehindMarker;
}

public class PlayerUIMarkerSystem : MonoBehaviour
{
    public Camera playerCamera;
    public RectTransform canvasRect;

    [Header("Marker Setup")]
    public RectTransform markerPrefab;
    public RectTransform behindMarkerPrefab;
    public Vector3 worldOffset = Vector3.up * 2f;

    [Header("Targets")]
    public List<Transform> targets = new List<Transform>();

    [Header("Distance Fade (Front)")]
    public float nearFadeStart = 50f; // A
    public float nearFadeEnd = 10f;    // B

    [Header("Distance Fade (Behind)")]
    public float farFadeStart = 25f;  // C
    public float farFadeEnd = 50f;    // D

    [Header("Fade")]
    public float fadeSpeed = 5f;

    private Dictionary<Transform, MarkerData> activeMarkers = new();
    private bool active = true;

    void Start()
    {
        if (playerCamera != null && markerPrefab != null && targets.Count > 0)
        {
            InitializeMarkers();
        }
        else
        {
            Debug.LogWarning("PlayerUIMarkerSystem: Missing references or targets.");
        }
    }

    public void SetActive(bool isActive)
    {
        active = isActive;
        foreach (var marker in activeMarkers.Values)
        {
            marker.canvasGroup.alpha = isActive ? marker.targetAlpha : 0f;
        }
    }

    public void ManualInitialize(Camera cam, RectTransform canvas, List<Transform> targetList) {
        playerCamera = cam;
        canvasRect = canvas;
        targets = targetList;
        if (playerCamera != null && markerPrefab != null && targets.Count > 0) {
            InitializeMarkers();
        } else { 
            Debug.LogWarning("PlayerUIMarkerSystem: Missing references or targets. Markers will not be initialized.");
        }
    }

    void InitializeMarkers()
    {
        foreach (var t in targets)
        {
            if (t == null) continue;
            if (activeMarkers.ContainsKey(t)) continue;

            RectTransform instance = Instantiate(markerPrefab, canvasRect);
            CanvasGroup cg = instance.GetComponent<CanvasGroup>();

            MarkerData data = new MarkerData
            {
                rect = instance,
                canvasGroup = cg,
                targetAlpha = 0f,
                isBehindMarker = false
            };

            activeMarkers.Add(t, data);
        }
    }

    void LateUpdate()
    {
        if (active)
        {
            if (activeMarkers.Count == 0) return;

            foreach (var pair in activeMarkers)
            {
                Transform target = pair.Key;
                MarkerData marker = pair.Value;

                if (target == null)
                {
                    marker.targetAlpha = 0f;
                    continue;
                }

                Vector3 worldPos = target.position + worldOffset;
                Vector3 viewportPos = playerCamera.WorldToViewportPoint(worldPos);

                bool isBehind = viewportPos.z < 0;
                float distance = Vector3.Distance(playerCamera.transform.position, target.position);

                float margin = 0.05f; // margine in viewport (5%)

                if (isBehind)
                {
                    SwapMarkerPrefab(marker, true);

                    viewportPos.x = 1f - viewportPos.x;
                    viewportPos.x = Mathf.Clamp(viewportPos.x, margin, 1f - margin);
                    viewportPos.y = margin;

                    marker.targetAlpha = GetBehindAlpha(distance);
                }
                else
                {
                    SwapMarkerPrefab(marker, false);

                    bool isInside =
                        viewportPos.x >= 0 && viewportPos.x <= 1 &&
                        viewportPos.y >= 0 && viewportPos.y <= 1;

                    if (isInside && IsVisible(target))
                    {
                        viewportPos.x = Mathf.Clamp(viewportPos.x, margin, 1f - margin);
                        viewportPos.y = Mathf.Clamp(viewportPos.y, margin, 1f - margin);

                        marker.targetAlpha = GetFrontAlpha(distance);
                    }
                    else
                    {
                        marker.targetAlpha = 0f;
                    }
                }

                // aggiorna posizione
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    playerCamera.ViewportToScreenPoint(viewportPos),
                    playerCamera,
                    out Vector2 localPos
                );

                marker.rect.localPosition = localPos;

                // fade smooth
                marker.canvasGroup.alpha = Mathf.Lerp(
                    marker.canvasGroup.alpha,
                    marker.targetAlpha,
                    Time.deltaTime * fadeSpeed
                );
            }
        }        
    }

    void SwapMarkerPrefab(MarkerData marker, bool shouldBeBehind)
    {
        if (marker.isBehindMarker == shouldBeBehind)
            return;

        Destroy(marker.rect.gameObject);

        RectTransform newInstance = Instantiate(
            shouldBeBehind ? behindMarkerPrefab : markerPrefab,
            canvasRect
        );

        marker.rect = newInstance;
        marker.canvasGroup = newInstance.GetComponent<CanvasGroup>();
        marker.isBehindMarker = shouldBeBehind;
    }

    float GetFrontAlpha(float distance)
    {
        if (distance <= nearFadeEnd) return 0f;
        if (distance >= nearFadeStart) return 1f;

        return Mathf.InverseLerp(nearFadeEnd, nearFadeStart, distance);
    }

    float GetBehindAlpha(float distance)
    {
        if (distance <= farFadeStart) return 1f;
        if (distance >= farFadeEnd) return 0f;

        return 1f - Mathf.InverseLerp(farFadeStart, farFadeEnd, distance);
    }

    bool IsVisible(Transform target)
    {
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = (target.position - origin).normalized;
        float distance = Vector3.Distance(origin, target.position);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            return hit.transform == target;
        }

        return true;
    }
}