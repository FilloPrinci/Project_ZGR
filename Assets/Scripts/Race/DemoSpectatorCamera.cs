using UnityEngine;

// Drives the external "broadcast" camera used by Demo Mode (RaceMode.Demo) to follow a randomly
// chosen CPU vehicle. Cycles between the four VehicleAnchors spectator points (results-style side
// view, its mirror, and two chase angles) on a fixed interval, picking a different one than the
// current angle each time so it doesn't repeat back-to-back - the classic "TV director cutting
// between cameras" attract-mode look.
//
// Snaps position/rotation directly from the current anchor every frame rather than smoothing -
// the anchor itself already moves smoothly (it's a child of the followed vehicle), matching the
// same snap pattern FeedBackManager already uses for the single-player results camera. Switching
// anchors is a hard cut, not a pan, matching how these attract-mode cameras read in games like
// F-Zero GX / Mario Kart.
public class DemoSpectatorCamera : MonoBehaviour
{
    [Tooltip("Seconds between camera angle switches.")]
    public float minSwitchInterval = 6f;
    public float maxSwitchInterval = 8f;

    private Transform[] anchors;
    private int currentAnchorIndex;
    private float timeUntilSwitch;

    public void Follow(VehicleAnchors vehicleAnchors)
    {
        anchors = new Transform[]
        {
            vehicleAnchors.OutRace_CameraPivot,
            vehicleAnchors.OutRace_CameraPivot_Mirrored,
            vehicleAnchors.DemoChase_Front,
            vehicleAnchors.DemoChase_Rear,
        };

        currentAnchorIndex = Random.Range(0, anchors.Length);
        timeUntilSwitch = Random.Range(minSwitchInterval, maxSwitchInterval);
        SnapToCurrentAnchor();
    }

    private void Update()
    {
        if (anchors == null)
        {
            return;
        }

        timeUntilSwitch -= Time.deltaTime;
        if (timeUntilSwitch <= 0f)
        {
            // Never immediately repeat the angle that's already showing.
            int nextIndex;
            do
            {
                nextIndex = Random.Range(0, anchors.Length);
            } while (nextIndex == currentAnchorIndex && anchors.Length > 1);

            currentAnchorIndex = nextIndex;
            timeUntilSwitch = Random.Range(minSwitchInterval, maxSwitchInterval);
        }
    }

    private void LateUpdate()
    {
        if (anchors == null)
        {
            return;
        }

        SnapToCurrentAnchor();
    }

    private void SnapToCurrentAnchor()
    {
        Transform anchor = anchors[currentAnchorIndex];
        if (anchor == null)
        {
            return;
        }

        transform.SetPositionAndRotation(anchor.position, anchor.rotation);
    }
}
