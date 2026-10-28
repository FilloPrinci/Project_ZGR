using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Looping title screen music that keeps playing, uninterrupted, into Demo Mode races (attract
// mode): the demo race never plays its own track soundtrack (see RaceManager.OnRaceStart), this
// one carries over instead. Survives scene loads (DontDestroyOnLoad) only while the game stays
// in "attract mode" - the title screen itself or a RaceMode.Demo race - and fades out as soon as
// any other scene loads (main menu, a real race).
//
// Coming back to the title screen from a demo race keeps the already-playing instance (the
// title screen's own copy destroys itself as a duplicate), so the loop never restarts.
[RequireComponent(typeof(AudioSource))]
public class TitleMusicPlayer : MonoBehaviour
{
    public static TitleMusicPlayer Instance { get; private set; }

    public string titleSceneName = "TitleScreen";
    public float fadeOutDuration = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad only works on root objects.
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.spatialBlend = 0f; // 2D: must not depend on where the AudioListener is
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // sceneLoaded runs after the new scene's Awake(), so RaceManager has already read its mode
    // from RaceSettings by now.
    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (scene.name == titleSceneName)
        {
            return;
        }

        RaceManager raceManager = RaceManager.Instance;
        if (raceManager != null && raceManager.GetCurrentRaceMode() == RaceMode.Demo)
        {
            return;
        }

        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;

        float startVolume = audioSource.volume;
        float time = 0f;
        while (time < fadeOutDuration)
        {
            time += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeOutDuration);
            yield return null;
        }

        Destroy(gameObject);
    }
}
