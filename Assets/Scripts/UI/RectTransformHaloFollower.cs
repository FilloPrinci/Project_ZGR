using UnityEngine;

// Mirrors a target RectTransform's anchors/offsets every frame, inflated by a fixed padding -
// lets a sibling "halo" layer track a dynamically-resized element (e.g. the energy bar fill,
// resized by RaceGUI.ShowEnergy() changing its anchorMax.x) without that element's own code
// needing to know the halo exists.
//
// Must be a SIBLING of the target, not a child: a child always renders in front of its parent's
// own Graphic in Unity UI regardless of sibling index (confirmed live elsewhere in this project -
// see DelayedActionButton's border/fill comments), so nesting the halo inside the bar would put
// it on top instead of bleeding out from behind it. Render order (behind the bar, in front of
// its static background) is instead controlled by this GameObject's own sibling index in the
// Inspector/prefab.
[ExecuteAlways]
public class RectTransformHaloFollower : MonoBehaviour
{
    public RectTransform target;
    public float padding = 14f;

    private RectTransform rt;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (rt == null)
        {
            rt = GetComponent<RectTransform>();
        }

        rt.anchorMin = target.anchorMin;
        rt.anchorMax = target.anchorMax;
        rt.offsetMin = target.offsetMin - new Vector2(padding, padding);
        rt.offsetMax = target.offsetMax + new Vector2(padding, padding);
    }
}
