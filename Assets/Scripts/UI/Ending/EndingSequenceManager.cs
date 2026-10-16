using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// What the session that just ended was, captured by RaceGUI right before RaceSettings gets
// reset (see RaceGUI.OnRaceExit) so the ending scene can still tailor its thank-you message.
public static class EndingSequenceContext
{
    public static bool HasData { get; private set; }
    public static SessionMode SessionMode { get; private set; }
    public static string TrophyName { get; private set; }
    public static string TrackName { get; private set; }

    public static void Capture(RaceSettings raceSettings)
    {
        HasData = raceSettings != null;
        if (!HasData) return;

        SessionMode = raceSettings.IsTrophyActive() ? SessionMode.Trophy : SessionMode.SingleRace;
        TrophyName = raceSettings.GetSelectedTrophyName();
        TrackName = raceSettings.GetSelectedRaceTrackDisplayName();
    }

    public static void Clear()
    {
        HasData = false;
    }
}

// Drives the "Ending" scene, loaded by RaceGUI.OnRaceExit once a single race or a whole
// trophy has been completed: thank-you screen first, then (trophy only) the scrolling end credits (content
// from CreditsConfig), then back to SceneReferences.startScene.
//
// Input (polled directly, like LoadSceneOnAnyKey, since the race's PlayerInput instances are
// gone by now): any button skips the thank-you screen; during the credits, holding any button
// fast-forwards and Esc / gamepad Start skips them entirely. Each phase ignores input for
// inputDelay seconds and until every button has been released once, so the press that
// confirmed "Exit" on the results screen doesn't immediately skip anything.
public class EndingSequenceManager : MonoBehaviour
{
    [Header("Credits data")]
    public CreditsConfig creditsConfig;

    [Header("Thank you screen")]
    public CanvasGroup thankYouGroup;
    public TextMeshProUGUI thankYouTitleText;
    public TextMeshProUGUI thankYouMessageText;
    public string thankYouTitle = "Thank you!";
    // {track} and {trophy} are replaced with the names of what was just completed.
    [TextArea(2, 6)] public string singleRaceMessage = "You completed {track}.\nThanks for playing Project ZGR!";
    [TextArea(2, 6)] public string trophyMessage = "You completed {trophy}.\nThanks for playing Project ZGR!";
    // Used when there's no track/trophy name to show (e.g. this scene played directly).
    [TextArea(2, 6)] public string genericMessage = "Thanks for playing Project ZGR!";
    public float thankYouDuration = 5f;

    [Header("Credits screen")]
    public CanvasGroup creditsGroup;
    public RectTransform creditsViewport;
    public RectTransform creditsContent;
    // Inactive objects in the scene, cloned once per credits line - restyle them freely.
    public TextMeshProUGUI titleTemplate;
    public TextMeshProUGUI sectionTitleTemplate;
    public TextMeshProUGUI roleTemplate;
    public TextMeshProUGUI namesTemplate;
    public Image imageTemplate;
    public GameObject skipHint;

    [Header("Transitions")]
    public float fadeDuration = 1f;
    public float inputDelay = 0.75f;

    [Header("Audio")]
    public AudioSource musicSource;

    [Header("Fallback")]
    // Used only when SceneReferences isn't loaded (e.g. playing this scene directly in Editor).
    public string fallbackMenuScene = "CustomUI_System";

    private float phaseStartTime;
    private bool waitingForRelease;
    private RectTransform closingLine;

    private void Start()
    {
        if (creditsConfig == null)
        {
            creditsConfig = GetComponent<CreditsConfig>();
        }

        SetGroupAlpha(thankYouGroup, 0f);
        SetGroupAlpha(creditsGroup, 0f);
        if (skipHint != null) skipHint.SetActive(false);

        DeactivateTemplate(titleTemplate);
        DeactivateTemplate(sectionTitleTemplate);
        DeactivateTemplate(roleTemplate);
        DeactivateTemplate(namesTemplate);
        DeactivateTemplate(imageTemplate);

        if (musicSource != null && musicSource.clip != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }

        StartCoroutine(SequenceRoutine());
    }

    private IEnumerator SequenceRoutine()
    {
        yield return ThankYouRoutine();

        if (ShouldShowCredits())
        {
            yield return CreditsRoutine();
        }

        if (musicSource != null)
        {
            StartCoroutine(FadeOutMusic(fadeDuration));
        }

        EndingSequenceContext.Clear();
        LoadMenu();
    }

    // --- Thank you ---

    private IEnumerator ThankYouRoutine()
    {
        if (thankYouGroup == null) yield break;

        if (thankYouTitleText != null) thankYouTitleText.text = thankYouTitle;
        if (thankYouMessageText != null) thankYouMessageText.text = BuildThankYouMessage();

        yield return Fade(thankYouGroup, 0f, 1f, fadeDuration);

        BeginPhase();
        float elapsed = 0f;
        while (elapsed < thankYouDuration)
        {
            if (CanReadInput() && AnyButtonPressedThisFrame()) break;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        yield return Fade(thankYouGroup, 1f, 0f, fadeDuration);
    }

    private string BuildThankYouMessage()
    {
        if (!EndingSequenceContext.HasData)
        {
            return genericMessage;
        }

        bool isTrophy = EndingSequenceContext.SessionMode == SessionMode.Trophy;
        string name = isTrophy ? EndingSequenceContext.TrophyName : EndingSequenceContext.TrackName;
        if (string.IsNullOrEmpty(name))
        {
            return genericMessage;
        }

        string message = isTrophy ? trophyMessage : singleRaceMessage;
        return message.Replace("{trophy}", EndingSequenceContext.TrophyName).Replace("{track}", EndingSequenceContext.TrackName);
    }

    // --- Credits ---

    // Credits only follow a completed trophy; a single race gets just the thank-you screen.
    // No context means this scene was played directly in the Editor: show them, for testing.
    private bool ShouldShowCredits()
    {
        return !EndingSequenceContext.HasData || EndingSequenceContext.SessionMode == SessionMode.Trophy;
    }

    private IEnumerator CreditsRoutine()
    {
        if (creditsGroup == null || creditsContent == null || creditsViewport == null || creditsConfig == null)
        {
            Debug.LogWarning("[EndingSequenceManager] Credits references or CreditsConfig missing, skipping credits.");
            yield break;
        }

        BuildCredits();
        LayoutRebuilder.ForceRebuildLayoutImmediate(creditsContent);

        // creditsContent is anchored/pivoted at the viewport's top: y = 0 puts its top edge at
        // the viewport's top, y = -viewportHeight puts it just below the bottom edge.
        float viewportHeight = creditsViewport.rect.height;
        float contentHeight = creditsContent.rect.height;
        float startY = -viewportHeight;
        float endY = contentHeight;

        if (closingLine != null)
        {
            // Stop with the closing line centered on screen instead of scrolling it away.
            float lineTop = -closingLine.anchoredPosition.y - closingLine.rect.height * (1f - closingLine.pivot.y);
            endY = lineTop + closingLine.rect.height * 0.5f - viewportHeight * 0.5f;
        }

        SetContentY(startY);
        yield return Fade(creditsGroup, 0f, 1f, fadeDuration);

        BeginPhase();
        if (skipHint != null) skipHint.SetActive(true);

        float y = startY;
        bool skipped = false;
        while (y < endY)
        {
            if (CanReadInput() && SkipPressedThisFrame())
            {
                skipped = true;
                break;
            }

            float speed = creditsConfig.scrollSpeed;
            if (CanReadInput() && AnyButtonHeld())
            {
                speed *= creditsConfig.fastForwardMultiplier;
            }

            y = Mathf.Min(y + speed * Time.unscaledDeltaTime, endY);
            SetContentY(y);
            yield return null;
        }

        if (skipHint != null) skipHint.SetActive(false);

        if (!skipped && closingLine != null)
        {
            float elapsed = 0f;
            while (elapsed < creditsConfig.endHoldDuration)
            {
                if (CanReadInput() && AnyButtonPressedThisFrame()) break;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        yield return Fade(creditsGroup, 1f, 0f, fadeDuration);
    }

    private void BuildCredits()
    {
        for (int i = creditsContent.childCount - 1; i >= 0; i--)
        {
            Transform child = creditsContent.GetChild(i);
            if (IsTemplate(child)) continue;
            Destroy(child.gameObject);
        }

        if (creditsConfig.logo != null)
        {
            AddImage(creditsConfig.logo, creditsConfig.logoHeight);
            AddSpacer(creditsConfig.entrySpacing);
        }

        if (!string.IsNullOrEmpty(creditsConfig.gameTitle))
        {
            AddText(titleTemplate, creditsConfig.gameTitle);
        }

        foreach (CreditsSection section in creditsConfig.sections)
        {
            AddSpacer(creditsConfig.sectionSpacing);

            if (section.image != null)
            {
                AddImage(section.image, section.imageHeight);
                AddSpacer(creditsConfig.entrySpacing);
            }

            if (!string.IsNullOrEmpty(section.title))
            {
                AddText(sectionTitleTemplate, section.title);
                AddSpacer(creditsConfig.entrySpacing);
            }

            for (int i = 0; i < section.entries.Count; i++)
            {
                CreditsEntry entry = section.entries[i];
                if (!string.IsNullOrEmpty(entry.role)) AddText(roleTemplate, entry.role);
                if (!string.IsNullOrEmpty(entry.names)) AddText(namesTemplate, entry.names.Trim());
                if (i < section.entries.Count - 1) AddSpacer(creditsConfig.entrySpacing);
            }
        }

        closingLine = null;
        if (!string.IsNullOrEmpty(creditsConfig.closingMessage))
        {
            AddSpacer(creditsConfig.sectionSpacing * 2f);
            TextMeshProUGUI closing = AddText(titleTemplate, creditsConfig.closingMessage);
            closingLine = closing != null ? closing.rectTransform : null;
        }
    }

    private TextMeshProUGUI AddText(TextMeshProUGUI template, string text)
    {
        if (template == null) return null;

        TextMeshProUGUI line = Instantiate(template, creditsContent);
        line.gameObject.SetActive(true);
        line.text = text;
        return line;
    }

    private void AddImage(Sprite sprite, float height)
    {
        if (imageTemplate == null) return;

        Image image = Instantiate(imageTemplate, creditsContent);
        image.gameObject.SetActive(true);
        image.sprite = sprite;
        image.preserveAspect = true;

        LayoutElement layoutElement = image.GetComponent<LayoutElement>();
        if (layoutElement == null) layoutElement = image.gameObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = height;
        layoutElement.minHeight = height;
    }

    private void AddSpacer(float height)
    {
        if (height <= 0f) return;

        GameObject spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
        spacer.transform.SetParent(creditsContent, false);
        LayoutElement layoutElement = spacer.GetComponent<LayoutElement>();
        layoutElement.minHeight = height;
        layoutElement.preferredHeight = height;
    }

    private bool IsTemplate(Transform t)
    {
        return (titleTemplate != null && t == titleTemplate.transform)
            || (sectionTitleTemplate != null && t == sectionTitleTemplate.transform)
            || (roleTemplate != null && t == roleTemplate.transform)
            || (namesTemplate != null && t == namesTemplate.transform)
            || (imageTemplate != null && t == imageTemplate.transform);
    }

    private void SetContentY(float y)
    {
        Vector2 position = creditsContent.anchoredPosition;
        position.y = y;
        creditsContent.anchoredPosition = position;
    }

    // --- Input ---

    private void BeginPhase()
    {
        phaseStartTime = Time.unscaledTime;
        waitingForRelease = true;
    }

    private bool CanReadInput()
    {
        if (Time.unscaledTime - phaseStartTime < inputDelay) return false;

        if (waitingForRelease)
        {
            if (AnyButtonHeld()) return false;
            waitingForRelease = false;
        }
        return true;
    }

    private static bool AnyButtonPressedThisFrame()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) return true;

        foreach (Gamepad gamepad in Gamepad.all)
        {
            foreach (InputControl control in gamepad.allControls)
            {
                if (control is ButtonControl button && !button.synthetic && button.wasPressedThisFrame) return true;
            }
        }
        return false;
    }

    private static bool AnyButtonHeld()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.isPressed) return true;

        foreach (Gamepad gamepad in Gamepad.all)
        {
            foreach (InputControl control in gamepad.allControls)
            {
                if (control is ButtonControl button && !button.synthetic && button.isPressed) return true;
            }
        }
        return false;
    }

    private static bool SkipPressedThisFrame()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) return true;

        foreach (Gamepad gamepad in Gamepad.all)
        {
            if (gamepad.startButton.wasPressedThisFrame) return true;
        }
        return false;
    }

    // --- Helpers ---

    private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null) yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            yield return null;
        }
        group.alpha = to;
    }

    private IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = musicSource.volume;
        float t = 0f;
        while (t < duration && musicSource != null)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, Mathf.Clamp01(t / duration));
            yield return null;
        }
    }

    private static void SetGroupAlpha(CanvasGroup group, float alpha)
    {
        if (group != null) group.alpha = alpha;
    }

    private static void DeactivateTemplate(Component template)
    {
        if (template != null) template.gameObject.SetActive(false);
    }

    private void LoadMenu()
    {
        string menuScene = SceneReferences.Instance != null && !string.IsNullOrEmpty(SceneReferences.Instance.startScene)
            ? SceneReferences.Instance.startScene
            : fallbackMenuScene;

        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.LoadScene(menuScene);
        }
        else
        {
            SceneManager.LoadScene(menuScene);
        }
    }
}
