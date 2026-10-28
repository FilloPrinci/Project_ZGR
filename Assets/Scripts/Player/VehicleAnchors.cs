using UnityEngine;

public class VehicleAnchors : MonoBehaviour
{
    public Transform pivot;
    public Transform cameraPivot;
    public Transform Normal_cameraPivot;
    public Transform Turbo_cameraPivot;
    public Transform OutRace_CameraPivot;

    // Demo Mode spectator camera points (DemoSpectatorCamera cycles between all four). Same
    // mechanism as OutRace_CameraPivot - a fixed local offset child re-aimed at pivot every
    // frame - just at three more positions: mirrored to the opposite side, and two "chase" spots
    // (ahead/behind, offset to the side) for a broadcast-style varied set of external angles.
    public Transform OutRace_CameraPivot_Mirrored;
    public Transform DemoChase_Front;
    public Transform DemoChase_Rear;

    private Vector3 OutRace_StaticPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        OutRace_CameraPivot.LookAt(pivot);

        if (OutRace_CameraPivot_Mirrored != null) OutRace_CameraPivot_Mirrored.LookAt(pivot);
        if (DemoChase_Front != null) DemoChase_Front.LookAt(pivot);
        if (DemoChase_Rear != null) DemoChase_Rear.LookAt(pivot);
    }
}
