using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class LoadSceneOnAnyKey : MonoBehaviour
{
    [SerializeField] private string sceneName;

    // Lets code (e.g. RaceManager activating Demo Mode) point this at a scene at runtime,
    // instead of only being configurable from the Inspector.
    public void SetSceneName(string newSceneName)
    {
        sceneName = newSceneName;
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
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}