using Unity.VisualScripting;
using UnityEngine;

public enum CameraMode { 
    SinglePlayer,
    MultiPlayer2,
    MultiPlayer3,
    MultiPlayer4
}

public class PlayerStructure : MonoBehaviour
{
    public GameObject pivotPrefab;
    public PlayerController controller;
    public FeedBackManager feedBack;
    public GameObject canvasPrefab;
    public PlayerData data;
    public GameObject playerCamera;

    private GameObject canvasInstance;
    private GameObject pivotInstance;

    void Awake()
    {
        canvasInstance = Instantiate(canvasPrefab);
        pivotInstance = Instantiate(pivotPrefab);

        canvasInstance.GetComponent<RaceGUI>().currentPlayer = gameObject;
        controller.veichlePivot = pivotInstance.transform;
        controller.playerData = data;

        playerCamera = SpawnCamera(pivotInstance.GetComponent<VeichleAnchors>().cameraPivot);
        feedBack.playerCamera = playerCamera;
    }

    GameObject SpawnCamera(Transform anchor) {
        GameObject cameraObject = new GameObject(data.name + "Camera");
        Camera cam = cameraObject.AddComponent<Camera>();
        cam.AddComponent<CameraController>();
        cam.GetComponent<CameraController>().cameraDesiredPosition = anchor;
        cameraObject.SetActive(false);

        return cameraObject;
    }

    public void ActivatePlayerCamera(CameraMode mode)
    {
        switch (mode) {
            case CameraMode.SinglePlayer:
                // camera is full screen 
                break;
            case CameraMode.MultiPlayer2:
                // resize to half screen

                // position depends from playerData.InputIndex
                break;
            case CameraMode.MultiPlayer3:
                // resize to half screen

                // position depends from playerData.InputIndex
                break;
            case CameraMode.MultiPlayer4:
                // resize to quarter screen

                // position depends from playerData.InputIndex
                break;

        }

        playerCamera.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
