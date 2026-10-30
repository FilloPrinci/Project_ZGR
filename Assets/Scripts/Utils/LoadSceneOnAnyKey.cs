using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class LoadSceneOnAnyKey : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private bool fadeTransition = false;

    // Lets code (e.g. RaceManager activating Demo Mode) point this at a scene at runtime,
    // instead of only being configurable from the Inspector.
    public void SetSceneName(string newSceneName, bool useFadeTransition = false)
    {
        sceneName = newSceneName;
        fadeTransition = useFadeTransition;
    }

    private void Update()
    {
        // Keyboard
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            LoadScene();
            return;
        }

        // Gamepad
        if (Gamepad.current != null)
        {
            foreach (var control in Gamepad.current.allControls)
            {
                if (control is ButtonControl button && button.wasPressedThisFrame)
                {
                    LoadScene();
                    return;
                }
            }
        }
    }

    private void LoadScene()
    {
        // Only trigger once: the load takes several frames, and other scripts check this to know
        // a scene change is already underway (TitleScreenIdleTimer, RaceManager's Demo Mode exit).
        enabled = false;

        if (LoadingScreenManager.Instance != null)
        {
            if (fadeTransition)
            {
                LoadingScreenManager.Instance.FadeToScene(sceneName, showLoadingScreen: true);
            }
            else
            {
                LoadingScreenManager.Instance.LoadScene(sceneName);
            }
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}