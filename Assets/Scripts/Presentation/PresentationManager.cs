using UnityEngine;

public class PresentationManager : MonoBehaviour
{
    public System.Collections.Generic.List<GameObject> presentationPointList;
    public float singlePointDuration;

    private Transform currentCameraPoint;
    private Camera currentCamera;

    public void SetCamera(Camera newCamera) {
        currentCamera = newCamera;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (presentationPointList != null) { 
            currentCameraPoint = presentationPointList[0].GetComponent<RotateCameraPoint>().cameraPoint;
            
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

}
