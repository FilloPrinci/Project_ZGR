using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Persistent (child of the DontDestroyOnLoad "Settings" prefab) full-screen overlay shown
// around every scene load in the game. Centralizing it here means every caller just does
// LoadingScreenManager.Instance.LoadScene("SceneName") instead of calling SceneManager
// directly, so no transition is ever missing the loading screen.
public class LoadingScreenManager : MonoBehaviour
{
    public static LoadingScreenManager Instance { get; private set; }

    [Header("UI")]
    public GameObject panel;
    public TextMeshProUGUI loadingLabel;
    public RectTransform spinner;

    [Header("Animation")]
    public float spinnerDegreesPerSecond = 220f;
    public float dotsPerSecond = 2f;
    public string baseLabelText = "LOADING";

    private bool isShowing = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Duplicate LoadingScreenManager detected. Destroying extra instance.");
            Destroy(gameObject);
            return;
        }

        SetVisible(false);
    }

    private void Update()
    {
        if (!isShowing)
        {
            return;
        }

        if (spinner != null)
        {
            spinner.Rotate(0f, 0f, -spinnerDegreesPerSecond * Time.unscaledDeltaTime);
        }

        if (loadingLabel != null)
        {
            int dotCount = Mathf.FloorToInt(Time.unscaledTime * dotsPerSecond) % 4;
            loadingLabel.text = baseLabelText + new string('.', dotCount);
        }
    }

    // Shows the loading screen, loads the scene asynchronously, then hides it again once the
    // new scene has finished loading. Use this instead of calling SceneManager directly.
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        SetVisible(true);

        // Let the overlay actually render at least one frame before the load kicks off.
        yield return null;
        yield return null;

        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
        if (asyncOp == null)
        {
            Debug.LogError("[LoadingScreenManager] Could not start loading scene: " + sceneName);
            SetVisible(false);
            yield break;
        }

        while (!asyncOp.isDone)
        {
            yield return null;
        }

        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        isShowing = visible;
        if (panel != null)
        {
            panel.SetActive(visible);
        }
        if (visible && loadingLabel != null)
        {
            loadingLabel.text = baseLabelText;
        }
    }
}
