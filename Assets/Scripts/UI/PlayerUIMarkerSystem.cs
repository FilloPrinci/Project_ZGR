using System.Collections.Generic;
using UnityEngine;

public class PlayerUIMarkerSystem : MonoBehaviour
{
    public Camera playerCamera;
    public RectTransform canvasRect;

    [Header("Marker Setup")]
    public RectTransform markerPrefab;
    public Vector3 worldOffset = Vector3.up * 2f;

    [Header("Targets")]
    public List<Transform> targets = new List<Transform>();

    private Dictionary<Transform, RectTransform> activeMarkers = new Dictionary<Transform, RectTransform>();

    void Start()
    {
        InitializeMarkers();
    }

    void InitializeMarkers()
    {
        foreach (var t in targets)
        {
            if (t == null) continue;

            RectTransform markerInstance = Instantiate(markerPrefab, canvasRect);
            activeMarkers.Add(t, markerInstance);
        }
    }

    void LateUpdate()
    {
        foreach (var pair in activeMarkers)
        {
            Transform target = pair.Key;
            RectTransform marker = pair.Value;

            if (target == null)
            {
                marker.gameObject.SetActive(false);
                continue;
            }

            Vector3 worldPos = target.position + worldOffset;
            Vector3 screenPos = playerCamera.WorldToScreenPoint(worldPos);

            float margin = 50f;
            bool isBehind = screenPos.z < 0;

            // 👉 CASO 1: dietro la camera → sempre visibile in basso
            if (isBehind)
            {
                // correggi inversione orizzontale
                screenPos.x = Screen.width - screenPos.x;

                screenPos.x = Mathf.Clamp(screenPos.x, margin, Screen.width - margin);
                screenPos.y = margin;

                marker.gameObject.SetActive(true);
            }
            else
            {
                // 👉 controlla se è dentro lo schermo
                bool isInside =
                    screenPos.x >= 0 && screenPos.x <= Screen.width &&
                    screenPos.y >= 0 && screenPos.y <= Screen.height;

                if (!isInside)
                {
                    // 👉 CASO 2: fuori schermo davanti → NASCONDI
                    marker.gameObject.SetActive(false);
                    continue;
                }

                // 👉 CASO 3: dentro schermo → normale
                screenPos.x = Mathf.Clamp(screenPos.x, margin, Screen.width - margin);
                screenPos.y = Mathf.Clamp(screenPos.y, margin, Screen.height - margin);

                marker.gameObject.SetActive(true);
            }

            // 👉 conversione in UI space
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                playerCamera,
                out Vector2 localPos
            );

            marker.localPosition = localPos;
        }
    }
}