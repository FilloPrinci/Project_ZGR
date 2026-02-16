using UnityEngine;

public class VeichleAnchors : MonoBehaviour
{
    public Transform pivot;
    public Transform cameraPivot;
    public Transform Normal_cameraPivot;
    public Transform Turbo_cameraPivot;
    public Transform OutRace_CameraPivot;

    private Vector3 OutRace_StaticPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (OutRace_CameraPivot != null) { 
            OutRace_StaticPosition = OutRace_CameraPivot.localPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        OutRace_CameraPivot.position = transform.position + OutRace_StaticPosition;
        OutRace_CameraPivot.LookAt(pivot);
    }
}
