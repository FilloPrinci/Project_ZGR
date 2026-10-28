using UnityEngine;

// Fades a CanvasGroup's alpha in a smooth pulse - used for the "Press Any Button" prompt on the
// title screen, and reused identically on the overlay shown during Demo Mode races.
[RequireComponent(typeof(CanvasGroup))]
public class BlinkingPressStart : MonoBehaviour
{
    public float blinkSpeed = 2f; // pulse cycles per second
    [Range(0f, 1f)] public float minAlpha = 0.15f;
    [Range(0f, 1f)] public float maxAlpha = 1f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.time * blinkSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
    }
}
