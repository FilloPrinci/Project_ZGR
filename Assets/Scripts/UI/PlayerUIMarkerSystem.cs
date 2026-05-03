using System.Collections.Generic;
using TMPro;
using UnityEngine;

class MarkerData
{
    public RectTransform frontRect;
    public RectTransform behindRect;

    public CanvasGroup frontCG;
    public CanvasGroup behindCG;

    public TextMeshProUGUI frontText;

    public float targetAlpha;
    public PlayerController controller;
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
    public float nearFadeStart = 50f;
    public float nearFadeEnd = 10f;

    [Header("Distance Fade (Behind)")]
    public float farFadeStart = 25f;
    public float farFadeEnd = 50f;

    [Header("Fade")]
    public float fadeSpeed = 5f;

    private Dictionary<Transform, MarkerData> activeMarkers = new();
    private bool active = true;

    void Start()
    {
        if (playerCamera != null && markerPrefab != null && targets.Count > 0)
            InitializeMarkers();
        else
            Debug.LogWarning("PlayerUIMarkerSystem: Missing references or targets.");
    }

    public void SetActive(bool isActive)
    {
        active = isActive;

        foreach (var marker in activeMarkers.Values)
        {
            marker.frontCG.alpha = isActive ? marker.targetAlpha : 0f;
            marker.behindCG.alpha = isActive ? marker.targetAlpha : 0f;
        }
    }

    public void ManualInitialize(Camera cam, RectTransform canvas, List<Transform> targetList)
    {
        playerCamera = cam;
        canvasRect = canvas;
        targets = targetList;

        if (playerCamera != null && markerPrefab != null && targets.Count > 0)
            InitializeMarkers();
        else
            Debug.LogWarning("PlayerUIMarkerSystem: Missing references or targets.");
    }

    void InitializeMarkers()
    {
        foreach (var t in targets)
        {
            if (t == null || activeMarkers.ContainsKey(t))
                continue;

            // FRONT
            RectTransform frontInstance = Instantiate(markerPrefab, canvasRect);
            CanvasGroup frontCG = frontInstance.GetComponent<CanvasGroup>();
            TextMeshProUGUI frontText = frontInstance.GetComponentInChildren<TextMeshProUGUI>();

            // BEHIND
            RectTransform behindInstance = Instantiate(behindMarkerPrefab, canvasRect);
            CanvasGroup behindCG = behindInstance.GetComponent<CanvasGroup>();

            behindInstance.gameObject.SetActive(false);

            MarkerData data = new MarkerData
            {
                frontRect = frontInstance,
                behindRect = behindInstance,
                frontCG = frontCG,
                behindCG = behindCG,
                frontText = frontText,
                targetAlpha = 0f,
                controller = t.GetComponent<PlayerController>()
            };

            activeMarkers.Add(t, data);
        }
    }

    void LateUpdate()
    {
        if (!active || activeMarkers.Count == 0)
            return;

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

            float margin = 0.05f;

            RectTransform activeRect;
            CanvasGroup activeCG;

            if (isBehind)
            {
                marker.frontRect.gameObject.SetActive(false);
                marker.behindRect.gameObject.SetActive(true);

                activeRect = marker.behindRect;
                activeCG = marker.behindCG;

                viewportPos.x = 1f - viewportPos.x;
                viewportPos.x = Mathf.Clamp(viewportPos.x, margin, 1f - margin);
                viewportPos.y = margin;

                marker.targetAlpha = GetBehindAlpha(distance);
            }
            else
            {
                marker.frontRect.gameObject.SetActive(true);
                marker.behindRect.gameObject.SetActive(false);

                activeRect = marker.frontRect;
                activeCG = marker.frontCG;

                bool isInside =
                    viewportPos.x >= 0 && viewportPos.x <= 1 &&
                    viewportPos.y >= 0 && viewportPos.y <= 1;

                if (isInside && IsVisible(target))
                {
                    viewportPos.x = Mathf.Clamp(viewportPos.x, margin, 1f - margin);
                    viewportPos.y = Mathf.Clamp(viewportPos.y, margin, 1f - margin);

                    marker.targetAlpha = GetFrontAlpha(distance);

                    // TEXT UPDATE
                    if (marker.frontText != null && marker.controller != null)
                    {
                        int position = marker.controller.GetCurrentPositionInRace();
                        marker.frontText.text = position != 0 ? position.ToString() : "-";
                    }
                }
                else
                {
                    marker.targetAlpha = 0f;
                }
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                playerCamera.ViewportToScreenPoint(viewportPos),
                playerCamera,
                out Vector2 localPos
            );

            activeRect.localPosition = localPos;

            activeCG.alpha = Mathf.Lerp(
                activeCG.alpha,
                marker.targetAlpha,
                Time.deltaTime * fadeSpeed
            );
        }
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
            return hit.transform == target;

        return true;
    }
}