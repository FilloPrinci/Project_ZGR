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
    }

    // Update is called once per frame
    void Update()
    {
        OutRace_CameraPivot.LookAt(pivot);
    }
}
