using UnityEngine;
using TMPro;


public class PresentationManager : MonoBehaviour
{
    public System.Collections.Generic.List<GameObject> presentationPointList;
    public float singlePointDuration;
    public GameObject globalRaceCanvasPrefab;

    private Transform currentCameraPoint;
    private Camera currentCamera;
    private GameObject globalRaceCanvasInstance;

    public void SetCamera(Camera newCamera) {
        currentCamera = newCamera;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        globalRaceCanvasInstance = GameObject.Instantiate(globalRaceCanvasPrefab);

        if (presentationPointList != null) { 
            currentCameraPoint = presentationPointList[0].GetComponent<RotateCameraPoint>().cameraPoint;
            
        }

        RaceSettings currentRaceSettings = RaceSettings.Instance;
        if (currentRaceSettings == null)
        {
            Debug.Log($"[PresentationManager] : No RaceSettings Instance found, using editor settings");
        }
        else
        {
            Debug.Log($"[PresentationManager] : RaceSettings Instance found, using player configured settings");
            globalRaceCanvasInstance.GetComponent<GlobalRaceCanvasStructure>().raceTrackNameText.text = currentRaceSettings.GetSelectedRaceTrack();

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentCamera != null)
        {
            currentCamera.transform.position = currentCameraPoint.position;
            currentCamera.transform.rotation = currentCameraPoint.rotation;
        }
    }

    public void OnStartPresentation()
    {
        globalRaceCanvasInstance.SetActive(true);
    }

    public void OnEndPresentation()
    {
        globalRaceCanvasInstance.SetActive(false);
    }

}
