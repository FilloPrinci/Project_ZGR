using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class LoadSceneOnAnyKey : MonoBehaviour
{
    [SerializeField] private string sceneName;

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
        SceneManager.LoadScene(sceneName);
    }
}