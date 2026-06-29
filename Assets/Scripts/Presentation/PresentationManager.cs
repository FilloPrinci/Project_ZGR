using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum CameraMovementType
{
    None = 0,
    Orbit,
    ZoomIn,
    ZoomOut,
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight
}

[System.Serializable]
public class CameraMovement
{
    public CameraMovementType type;
    public Transform originTransform;
    public float factor;
    public float duration;
}

public class PresentationManager : MonoBehaviour
{
    public List<CameraMovement> presentationMovements;

    public List<GameObject> presentationPointList;
    public float singlePointDuration;
    public GameObject globalRaceCanvasPrefab;

    private Transform currentCameraPoint;
    private Camera currentCamera;
    private GameObject globalRaceCanvasInstance;

    private int currentMovementIndex = 0;

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
            if (presentationMovements != null && presentationMovements.Count > 0) {
                StartCoroutine(FollowPresentationMovementsList());
            }
            else
            {
                currentCamera.transform.position = currentCameraPoint.position;
                currentCamera.transform.rotation = currentCameraPoint.rotation;
            }
        }
    }

    public void  PresentMovement(CameraMovement cameraMovement)
    {
        float duration = cameraMovement.duration;
        float elapsed = 0f;

        while(elapsed < duration)
        {
            float t = elapsed / duration;

            switch (cameraMovement.type)
            {
                case CameraMovementType.None:
                    currentCamera.transform.position = cameraMovement.originTransform.position;
                    currentCamera.transform.rotation = cameraMovement.originTransform.rotation;
                    break;
                case CameraMovementType.Orbit:
                    // Implement orbit logic here
                    break;
                case CameraMovementType.ZoomIn:
                    // Implement zoom in logic here
                    break;
                case CameraMovementType.ZoomOut:
                    // Implement zoom out logic here
                    break;
                case CameraMovementType.MoveUp:
                    // Implement move up logic here
                    break;
                case CameraMovementType.MoveDown:
                    // Implement move down logic here
                    break;
                case CameraMovementType.MoveLeft:
                    // Implement move left logic here
                    break;
                case CameraMovementType.MoveRight:
                    // Implement move right logic here
                    break;
            }

            elapsed += Time.deltaTime;
        }

        Debug.Log("Movement presented");
    }

    public void OnStartPresentation()
    {
        globalRaceCanvasInstance.SetActive(true);
    }

    public void OnEndPresentation()
    {
        globalRaceCanvasInstance.SetActive(false);
    }

    private IEnumerator FollowPresentationMovementsList()
    {
        for(int i = 0; i < presentationMovements.Count; i++)
        {
            CameraMovement movement = presentationMovements[i];
            PresentMovement(movement);
        }

        yield return null;

    }

}
