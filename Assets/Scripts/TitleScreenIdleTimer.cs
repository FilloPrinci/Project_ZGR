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
public class TitleScreenIdleTimer : MonoBehaviour
{
    public float idleSeconds = 10f;

    private float idleTimer;

    private void Update()
    {
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

        string trackSceneName = raceSettings.GetSelectedRaceTrack();

        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.LoadScene(trackSceneName);
        }
        else
        {
            SceneManager.LoadScene(trackSceneName);
        }
    }
}
