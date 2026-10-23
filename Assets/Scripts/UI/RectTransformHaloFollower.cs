using UnityEngine;

// Mirrors a target RectTransform every frame to position ONE directional halo piece (Up, Down,
// or Right) that bleeds outward from a single side of it - so three siblings using this same
// script, one per direction, reproduce a glow that fades only along the one axis that actually
// faces away from the bar, instead of a single inflated rect fading on all four sides (which
// fades its corners faster than its edge midpoints and reads as a rounded/circular blob - see
// EnergyBarUI.shader's per-side _Feather* properties, which this pairs with).
//
// Must be a SIBLING of the target, not a child: a child always renders in front of its parent's
// own Graphic in Unity UI regardless of sibling index (confirmed live elsewhere in this project -
// see DelayedActionButton's border/fill comments), so nesting a halo inside the bar would put it
// on top instead of bleeding out from behind it. Render order (behind the bar, in front of its
// static background) is instead controlled by this GameObject's own sibling index in the
// Inspector/prefab.
[ExecuteAlways]
public class RectTransformHaloFollower : MonoBehaviour
{
    public enum HaloDirection
    {
        Up,
        Down,
        Right,
    }

    public RectTransform target;
    public HaloDirection direction = HaloDirection.Up;

    // How far this piece reaches beyond the bar's own edge, in local/Canvas units.
    public float extent = 16f;

    // Right only: only bleed past the fill's leading edge while there's still track ahead of it -
    // once full, that edge IS the track's own right edge, and bleeding past it would spill
    // outside the bar's background entirely instead of looking like progress.
    [Range(0.9f, 1f)]
    public float fullThreshold = 0.995f;

    [SerializeField]
    private float fillFraction = 1f; // Right only - driven by RaceGUI.ShowEnergy() via SetFillFraction()

    private RectTransform rt;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public void SetFillFraction(float fraction)
    {
        fillFraction = Mathf.Clamp01(fraction);
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

        switch (direction)
        {
            case HaloDirection.Up:
                // Full width, matching the target's X anchors/offsets exactly (no horizontal
                // falloff - uniform intensity across the whole bar) - only stacked above it,
                // bottom edge flush against the bar's own top edge.
                rt.anchorMin = target.anchorMin;
                rt.anchorMax = target.anchorMax;
                rt.offsetMin = new Vector2(target.offsetMin.x, target.offsetMax.y);
                rt.offsetMax = new Vector2(target.offsetMax.x, target.offsetMax.y + extent);
                break;

            case HaloDirection.Down:
                // Mirror of Up - full width, stacked below, top edge flush against the bar's own
                // bottom edge.
                rt.anchorMin = target.anchorMin;
                rt.anchorMax = target.anchorMax;
                rt.offsetMin = new Vector2(target.offsetMin.x, target.offsetMin.y - extent);
                rt.offsetMax = new Vector2(target.offsetMax.x, target.offsetMin.y);
                break;

            case HaloDirection.Right:
                // Full height, matching the target's Y anchors/offsets exactly (no vertical
                // falloff), bleeding right from the fill's leading edge. The X anchor is pinned
                // to the target's OWN right edge (target.anchorMax.x), not copied from its left
                // (target.anchorMin.x, always 0) - anchoring from the left like Up/Down do would
                // make this rect span from the bar's own left edge all the way to the fill's
                // right edge (covering nearly the whole fill, hidden entirely underneath
                // EnergyFG, which renders in front) instead of starting AT that edge and
                // extending past it. Scaling by fillFraction (rather than an on/off switch at the
                // threshold) means it's naturally near-zero at low energy and ramps up with it,
                // instead of snapping to full size the instant there's any energy at all - and at
                // 0 energy the target itself is zero-width, so this starts flush against it with
                // nothing to bleed from anyway.
                float rightExtent = fillFraction < fullThreshold ? extent * fillFraction : 0f;
                rt.anchorMin = new Vector2(target.anchorMax.x, target.anchorMin.y);
                rt.anchorMax = new Vector2(target.anchorMax.x, target.anchorMax.y);
                rt.offsetMin = new Vector2(0f, target.offsetMin.y);
                rt.offsetMax = new Vector2(rightExtent, target.offsetMax.y);
                break;
        }
    }
}
