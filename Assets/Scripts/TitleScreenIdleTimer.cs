using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

// Attract-mode trigger for the title screen: if nobody touches any input for idleSeconds, picks
// a random track and loads it in RaceMode.Demo (an all-CPU race followed by a spectator camera -
// see RaceManager.ActivateDemoMode). Any input resets the timer, exactly like LoadSceneOnAnyKey
// (which is what actually moves the player from here into the main menu on an explicit press) -
// this script only cares about the ABSENCE of input, so both coexist on the title screen without
// conflicting.
//
// The demo track is loaded asynchronously in the background WITHOUT the full-screen
// LoadingScreenManager overlay: the title screen stays visible and animated, with only a small
// spinner in a corner, so the switch into the demo feels like an attract-mode cut rather than
// a regular scene transition.
public class TitleScreenIdleTimer : MonoBehaviour
{
    public float idleSeconds = 10f;

    [Header("Demo Loading Indicator")]
    // Small corner spinner, inactive by default, shown only while the demo track loads.
    public RectTransform demoLoadingSpinner;
    public float spinnerDegreesPerSecond = 220f;

    private float idleTimer;
    private bool isLoadingDemo = false;

    private void Awake()
    {
        if (demoLoadingSpinner != null)
        {
            demoLoadingSpinner.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isLoadingDemo)
        {
            if (demoLoadingSpinner != null)
            {
                demoLoadingSpinner.Rotate(0f, 0f, -spinnerDegreesPerSecond * Time.unscaledDeltaTime);
            }
            return;
        }

        if (AnyInputThisFrame())
        {
            idleTimer = 0f;
            return;
        }

        idleTimer += Time.deltaTime;
        if (idleTimer >= idleSeconds)
        {
            StartDemo();
        }
    }

    private bool AnyInputThisFrame()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            return true;
        }

        if (Gamepad.current != null)
        {
            foreach (var control in Gamepad.current.allControls)
            {
                if (control is ButtonControl button && button.wasPressedThisFrame)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void StartDemo()
    {
        idleTimer = 0f;

        RaceSettings raceSettings = RaceSettings.Instance;
        SceneReferences sceneReferences = SceneReferences.Instance;

        if (raceSettings == null || sceneReferences == null || sceneReferences.trackSceneDataList == null || sceneReferences.trackSceneDataList.Count == 0)
        {
            Debug.LogWarning("[TitleScreenIdleTimer] : Can't start Demo Mode - RaceSettings/SceneReferences/track list not available.");
            return;
        }

        raceSettings.ResetSettings();
        raceSettings.OnDemoModeSelect();
        raceSettings.SetSelectedDifficulty(GlobalDifficulty.normal);

        int randomTrackIndex = Random.Range(0, sceneReferences.trackSceneDataList.Count);
        raceSettings.OnRaceTrackSelect(randomTrackIndex);

        StartCoroutine(LoadDemoSceneRoutine(raceSettings.GetSelectedRaceTrack()));
    }

    private IEnumerator LoadDemoSceneRoutine(string trackSceneName)
    {
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(trackSceneName);
        if (asyncOp == null)
        {
            Debug.LogError("[TitleScreenIdleTimer] : Could not start loading demo scene: " + trackSceneName);
            yield break;
        }

        isLoadingDemo = true;

        // Once the async load has started it can't be cancelled, so "press any key" must not
        // queue a second (main menu) load on top of it - the press is simply ignored until the
        // demo is running, where any input returns to the main menu anyway.
        LoadSceneOnAnyKey anyKeyLoader = GetComponent<LoadSceneOnAnyKey>();
        if (anyKeyLoader != null)
        {
            anyKeyLoader.enabled = false;
        }

        if (demoLoadingSpinner != null)
        {
            demoLoadingSpinner.gameObject.SetActive(true);
        }

        // This object is destroyed together with the title screen when the demo scene activates.
        while (!asyncOp.isDone)
        {
            yield return null;
        }
    }
}
