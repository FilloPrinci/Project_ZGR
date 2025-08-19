using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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

    public GameObject GetCanvasInstance()
    {
        return canvasInstance;
    }

    void Awake()
    {
        Debug.Log("PlayerStructure Awake called for " + gameObject.name);

        pivotInstance = Instantiate(pivotPrefab);
        controller.veichlePivot = pivotInstance.transform;
        controller.playerData = data;
        controller.playerStructure = this;

        if (data.playerInputIndex != InputIndex.CPU)
        {
            canvasInstance = Instantiate(canvasPrefab);
            canvasInstance.GetComponent<RaceGUI>().currentPlayer = gameObject;
            playerCamera = SpawnCamera(pivotInstance.GetComponent<VeichleAnchors>().cameraPivot);
            feedBack.playerCamera = playerCamera;
            canvasInstance.GetComponent<Canvas>().worldCamera = playerCamera.GetComponent<Camera>();
            canvasInstance.GetComponent<Canvas>().planeDistance = 0.5f;
            canvasInstance.SetActive(false);
        }

        
    }

    GameObject SpawnCamera(Transform anchor) {
        GameObject cameraObject = new GameObject(data.name + "Camera");
        Camera cam = cameraObject.AddComponent<Camera>();

        // URP settings
        var cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
        cameraData.renderPostProcessing = true;
        cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;

        // Posizionamento
        cameraObject.transform.position = anchor.position;
        cameraObject.transform.rotation = anchor.rotation;

        // Controller
        var controller = cameraObject.AddComponent<CameraController>();
        controller.cameraDesiredPosition = anchor;

        cameraObject.SetActive(false);
        return cameraObject;
    }

    public void ActivatePlayerCamera(CameraMode mode)
    {
        Camera cam = playerCamera.GetComponent<Camera>();

        int playerIndex = (int)data.playerInputIndex;

        switch (mode)
        {
            case CameraMode.SinglePlayer:
                cam.rect = new Rect(0f, 0f, 1f, 1f); // fullscreen
                break;

            case CameraMode.MultiPlayer2:
                if (playerIndex == 0)
                {
                    cam.rect = new Rect(0f, 0.5f, 1f, 0.5f); // top half
                }
                else if (playerIndex == 1)
                {
                    cam.rect = new Rect(0f, 0f, 1f, 0.5f); // bottom half
                }
                break;

            case CameraMode.MultiPlayer3:
                if (playerIndex == 0)
                {
                    cam.rect = new Rect(0f, 0.5f, 0.5f, 0.5f); // top-left
                }
                else if (playerIndex == 1)
                {
                    cam.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f); // top-right
                }
                else if (playerIndex == 2)
                {
                    cam.rect = new Rect(0.25f, 0f, 0.5f, 0.5f); // centered bottom
                }
                break;

            case CameraMode.MultiPlayer4:
                switch (playerIndex)
                {
                    case 0:
                        cam.rect = new Rect(0f, 0.5f, 0.5f, 0.5f); // top-left
                        break;
                    case 1:
                        cam.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f); // top-right
                        break;
                    case 2:
                        cam.rect = new Rect(0f, 0f, 0.5f, 0.5f); // bottom-left
                        break;
                    case 3:
                        cam.rect = new Rect(0.5f, 0f, 0.5f, 0.5f); // bottom-right
                        break;
                }
                break;
        }

        playerCamera.SetActive(true);
        canvasInstance.SetActive(true);
    }

    public void OnRaceEndPhase(int winnerIndex)
    {
        Camera cam = playerCamera.GetComponent<Camera>();

        int playerIndex = (int)data.playerInputIndex;

        if(playerIndex == winnerIndex)
        {
            cam.rect = new Rect(0f, 0f, 1f, 1f); // fullscreen
            canvasInstance.SetActive(true);
        }
        else
        {
            playerCamera.SetActive(false); // hide camera for other players
            canvasInstance.SetActive(false);
        }

        
    }

    public void UpdatePlayerGUI(PlayerStats playerStats)
    {
        ItemType[] items = playerStats.itemBuffer.ToArray();

        for (int i = 0; i< items.Length; i++)
        {
            ItemType item = items[i];

            int energyRequired = (i + 1) * 20;

            if (playerStats.Energy >= energyRequired)
            {
                canvasInstance.GetComponent<RaceGUI>().SetItemPanelActive(i, true);
            }
            else
            {
                canvasInstance.GetComponent<RaceGUI>().SetItemPanelActive(i, false);
            }

            if (item == ItemType.UpgradeSpeed)
            {
                canvasInstance.GetComponent<RaceGUI>().SetItemPanelImage(i, 0);
            }
            else if (item == ItemType.UpgradeAcceleration)
            {
                canvasInstance.GetComponent<RaceGUI>().SetItemPanelImage(i, 1);
            }
            else if (item == ItemType.UpgradeManeuverability)
            {
                canvasInstance.GetComponent<RaceGUI>().SetItemPanelImage(i, 2);
            }
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
