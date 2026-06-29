using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public enum CameraMovementType
{
    SetTransform = 0,
    Orbit,
    ZoomIn,
    ZoomOut,
    MoveForward,
    MoveBackward,
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight
}

[System.Serializable]
public class FadeSettings
{
    public bool enabled = false;
    public float duration = 0.5f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
}

[System.Serializable]
public class CameraMovement
{
    public CameraMovementType type;
    public float duration;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // SetTransform: camera interpolates to this transform
    public Transform targetTransform;

    // Orbit: pivot = center; radius = distance; heightOffset = height above pivot; orbitDegrees = total rotation over duration
    public Transform pivot;
    public float radius;
    public float heightOffset;
    public float orbitDegrees;
    public bool startFromPivotRotation;

    // ZoomIn / ZoomOut: optional look-at target; FOV changes by fovDelta
    public Transform lookAtTarget;
    public float fovDelta;

    // Move* (all directions)
    public float distance;

    // Fade overlays that run concurrently with this movement
    public FadeSettings fadeIn;   // black → transparent, starts at beginning
    public FadeSettings fadeOut;  // transparent → black, ends at end
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
    private CanvasGroup fadeOverlay;
    private bool isPlaying = false;

    public void SetCamera(Camera newCamera)
    {
        currentCamera = newCamera;
    }

    void Start()
    {
        globalRaceCanvasInstance = Instantiate(globalRaceCanvasPrefab);

        if (presentationPointList != null && presentationPointList.Count > 0)
            currentCameraPoint = presentationPointList[0].GetComponent<RotateCameraPoint>().cameraPoint;

        RaceSettings currentRaceSettings = RaceSettings.Instance;
        if (currentRaceSettings == null)
            Debug.Log("[PresentationManager] : No RaceSettings Instance found, using editor settings");
        else
        {
            Debug.Log("[PresentationManager] : RaceSettings Instance found, using player configured settings");
            globalRaceCanvasInstance.GetComponent<GlobalRaceCanvasStructure>().raceTrackNameText.text = currentRaceSettings.GetSelectedRaceTrack();
        }
    }

    // Fallback: if no movements defined, follow the rotating camera point from presentationPointList
    void Update()
    {
        if (currentCamera != null && !isPlaying && (presentationMovements == null || presentationMovements.Count == 0) && currentCameraPoint != null)
        {
            currentCamera.transform.position = currentCameraPoint.position;
            currentCamera.transform.rotation = currentCameraPoint.rotation;
        }
    }

    public void OnStartPresentation()
    {
        globalRaceCanvasInstance.SetActive(true);
        if (presentationMovements != null && presentationMovements.Count > 0)
            StartCoroutine(FollowPresentationMovementsList());
    }

    public void OnEndPresentation()
    {
        StopAllCoroutines();
        isPlaying = false;

        if (fadeOverlay != null)
            fadeOverlay.alpha = 0f;

        globalRaceCanvasInstance.SetActive(false);
    }

    private IEnumerator FollowPresentationMovementsList()
    {
        isPlaying = true;
        for (int i = 0; i < presentationMovements.Count; i++)
            yield return StartCoroutine(PresentMovement(presentationMovements[i]));
        isPlaying = false;
        RaceManager.Instance.TriggerRaceEvent(RacePhaseEvent.PresentationEnd);
    }

    private IEnumerator PresentMovement(CameraMovement movement)
    {
        float elapsed = 0f;
        AnimationCurve curve = movement.curve ?? AnimationCurve.Linear(0, 0, 1, 1);

        Vector3 cameraStartPos = currentCamera.transform.position;
        Quaternion cameraStartRot = currentCamera.transform.rotation;
        float startFOV = currentCamera.fieldOfView;

        // Orbit pre-compute
        // startFromPivotRotation=true  → axis=pivot.up, angle 0 = -pivot.forward (in front of pivot)
        // startFromPivotRotation=false → axis=Vector3.up, angle derived from current camera XZ position
        float orbitStartAngle = 0f;
        if (movement.type == CameraMovementType.Orbit)
        {
            if (movement.pivot == null)
            {
                Debug.LogWarning("[PresentationManager] Orbit requires a Pivot transform.");
                yield break;
            }
            if (!movement.startFromPivotRotation)
            {
                Vector3 flatOffset = cameraStartPos - movement.pivot.position;
                flatOffset.y = 0f;
                orbitStartAngle = Mathf.Atan2(flatOffset.x, flatOffset.z) * Mathf.Rad2Deg;
            }
        }

        if (movement.fadeIn.enabled)
            StartCoroutine(PlayFade(movement.fadeIn, isFadeIn: true, delay: 0f));

        if (movement.fadeOut.enabled)
            StartCoroutine(PlayFade(movement.fadeOut, isFadeIn: false,
                delay: Mathf.Max(0f, movement.duration - movement.fadeOut.duration)));

        while (elapsed < movement.duration)
        {
            float t  = elapsed / movement.duration;
            float ct = curve.Evaluate(t);

            switch (movement.type)
            {
                case CameraMovementType.SetTransform:
                    if (movement.targetTransform != null)
                        currentCamera.transform.SetPositionAndRotation(
                            Vector3.Lerp(cameraStartPos, movement.targetTransform.position, ct),
                            Quaternion.Slerp(cameraStartRot, movement.targetTransform.rotation, ct));
                    break;

                case CameraMovementType.Orbit:
                    float currentAngle = orbitStartAngle + movement.orbitDegrees * ct;
                    Vector3 orbitDir, orbitUp;
                    if (movement.startFromPivotRotation)
                    {
                        orbitUp  = movement.pivot.up;
                        orbitDir = Quaternion.AngleAxis(currentAngle, orbitUp) * (-movement.pivot.forward);
                    }
                    else
                    {
                        orbitUp  = Vector3.up;
                        orbitDir = Quaternion.AngleAxis(currentAngle, orbitUp) * Vector3.forward;
                    }
                    currentCamera.transform.position = movement.pivot.position + orbitDir * movement.radius + orbitUp * movement.heightOffset;
                    currentCamera.transform.LookAt(movement.pivot.position, orbitUp);
                    break;

                case CameraMovementType.ZoomIn:
                    if (movement.lookAtTarget != null)
                        currentCamera.transform.LookAt(movement.lookAtTarget.position);
                    currentCamera.fieldOfView = Mathf.Lerp(startFOV, startFOV - movement.fovDelta, ct);
                    break;

                case CameraMovementType.ZoomOut:
                    if (movement.lookAtTarget != null)
                        currentCamera.transform.LookAt(movement.lookAtTarget.position);
                    currentCamera.fieldOfView = Mathf.Lerp(startFOV, startFOV + movement.fovDelta, ct);
                    break;

                case CameraMovementType.MoveForward:
                    currentCamera.transform.position = Vector3.Lerp(cameraStartPos, cameraStartPos + cameraStartRot * Vector3.forward * movement.distance, ct);
                    currentCamera.transform.rotation = cameraStartRot;
                    break;

                case CameraMovementType.MoveBackward:
                    currentCamera.transform.position = Vector3.Lerp(cameraStartPos, cameraStartPos + cameraStartRot * Vector3.back * movement.distance, ct);
                    currentCamera.transform.rotation = cameraStartRot;
                    break;

                case CameraMovementType.MoveUp:
                    currentCamera.transform.position = Vector3.Lerp(cameraStartPos, cameraStartPos + Vector3.up * movement.distance, ct);
                    currentCamera.transform.rotation = cameraStartRot;
                    break;

                case CameraMovementType.MoveDown:
                    currentCamera.transform.position = Vector3.Lerp(cameraStartPos, cameraStartPos + Vector3.down * movement.distance, ct);
                    currentCamera.transform.rotation = cameraStartRot;
                    break;

                case CameraMovementType.MoveLeft:
                    currentCamera.transform.position = Vector3.Lerp(cameraStartPos, cameraStartPos + cameraStartRot * Vector3.left * movement.distance, ct);
                    currentCamera.transform.rotation = cameraStartRot;
                    break;

                case CameraMovementType.MoveRight:
                    currentCamera.transform.position = Vector3.Lerp(cameraStartPos, cameraStartPos + cameraStartRot * Vector3.right * movement.distance, ct);
                    currentCamera.transform.rotation = cameraStartRot;
                    break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator PlayFade(FadeSettings fade, bool isFadeIn, float delay = 0f)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        EnsureFadeOverlay();
        float elapsed = 0f;
        AnimationCurve fadeCurve = fade.curve ?? AnimationCurve.Linear(0, 0, 1, 1);

        while (elapsed < fade.duration)
        {
            float ct = fadeCurve.Evaluate(elapsed / fade.duration);
            fadeOverlay.alpha = isFadeIn ? 1f - ct : ct;
            elapsed += Time.deltaTime;
            yield return null;
        }

        fadeOverlay.alpha = isFadeIn ? 0f : 1f;
    }

    private void EnsureFadeOverlay()
    {
        if (fadeOverlay != null) return;

        var root = new GameObject("PresentationFadeOverlay");
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        root.AddComponent<CanvasScaler>();
        root.AddComponent<GraphicRaycaster>();

        var panel = new GameObject("BlackPanel");
        panel.transform.SetParent(root.transform, false);
        var img = panel.AddComponent<Image>();
        img.color = Color.black;
        var rect = img.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        fadeOverlay = root.AddComponent<CanvasGroup>();
        fadeOverlay.alpha = 0f;
        fadeOverlay.blocksRaycasts = false;
    }
}
