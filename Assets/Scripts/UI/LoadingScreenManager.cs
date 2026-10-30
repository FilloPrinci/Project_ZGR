using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("Fade Transition")]
    public float fadeDuration = 0.5f;

    private bool isShowing = false;
    private CanvasGroup fadeOverlay;

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

    // Fades to black, loads the scene behind the black, then fades into the new scene. With
    // showLoadingScreen the regular loading screen is shown during the load (its panel is black
    // too, so swapping between it and the fade overlay is seamless).
    public void FadeToScene(string sceneName, bool showLoadingScreen)
    {
        StartCoroutine(FadeToSceneRoutine(sceneName, showLoadingScreen));
    }

    private IEnumerator FadeToSceneRoutine(string sceneName, bool showLoadingScreen)
    {
        if (fadeOverlay == null)
        {
            fadeOverlay = CreateFadeOverlay();
        }

        yield return FadeOverlayRoutine(0f, 1f);

        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
        if (asyncOp == null)
        {
            Debug.LogError("[LoadingScreenManager] Could not start loading scene: " + sceneName);
            fadeOverlay.alpha = 0f;
            yield break;
        }

        if (showLoadingScreen)
        {
            SetVisible(true);
            fadeOverlay.alpha = 0f;
        }

        while (!asyncOp.isDone)
        {
            yield return null;
        }

        fadeOverlay.alpha = 1f;
        SetVisible(false);

        // Let the new scene run its Start() and render once before revealing it.
        yield return null;

        yield return FadeOverlayRoutine(1f, 0f);
    }

    private IEnumerator FadeOverlayRoutine(float fromAlpha, float toAlpha)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            fadeOverlay.alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        fadeOverlay.alpha = toAlpha;
    }

    // Last sibling of this canvas, so it draws on top of the loading screen panel.
    private CanvasGroup CreateFadeOverlay()
    {
        GameObject overlayObject = new GameObject("FadeOverlay");
        overlayObject.transform.SetParent(transform, false);

        Image image = overlayObject.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;

        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        CanvasGroup canvasGroup = overlayObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        return canvasGroup;
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
