using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class RaceGUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject resultsPanel;
    public GameObject resultsPanelContent;
    public GameObject pauseMenuPanel;
    public GameObject raceDataPanel;
    public GameObject speedometerPanel;
    public GameObject statsPanel;
    public GameObject itemsPanel;
    public GameObject positionResultPanel;
    public GameObject currentPositionPanel;

    [Header("Texts")]
    public TextMeshProUGUI raceDataText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI speedometerText;
    public TextMeshProUGUI positionResultText;
    public TextMeshProUGUI currentPositionText;

    [Header("Trophy")]
    // Shown on the results panel only while a Trophy session is active: recaps which stage of
    // the trophy just finished. The totals themselves are a column in the results list instead
    // (see AnimateResultsPoints) so they don't need repeating here.
    public GameObject trophyRecapPanel;
    public TextMeshProUGUI trophyRecapText;
    // Green "continue to next track" button - only shown while the trophy isn't finished yet.
    // The red "exit to menu" button is a plain UnityEngine.UI.Button wired to OnRaceExit()
    // directly and needs no script reference: it's always visible.
    public GameObject continueButton;

    [Header("Results points animation")]
    public Color gainedPointsColor = new Color(0.4f, 1f, 0.4f);
    public float resultsRowStagger = 0.08f;
    public float pointsPopDuration = 0.25f;
    public float totalCountDuration = 0.35f;

    [Header("GameObjects")]
    public GameObject currentPlayer;
    public GameObject pauseMenuSelectionStart;
    public GameObject resultMenuSelectionStart;

    [Header("Colors")]
    public Color goldColor;
    public Color silverColor;
    public Color bronzeColor;
    public Color highlightColor;

    public GameObject finishLabel;
    public RectTransform energyBar;

    public GameObject[] itemPanelList;
    public GameObject[] itemPanelImageList;
    public List<Sprite> availableSprites;

    private RaceManager raceManager;
    private CountdownManager countdownManager;
    private PlayerData currentPlayerData;
    private PlayerStats currentPlayerStats;
    private SceneReferences sceneReferences;
    private RaceSettings raceSettings;

    private bool canShowRaceDataLines = true;
    private bool canShowSpeedometer = true;
    private bool canShowStats = true;
    private bool canShowResults = false;
    private bool canShowPositionResult = false;
    private bool canShowCountdown = false;
    private bool canShowPauseMenu = false;

    private List<string> humanNameIdList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        raceManager = RaceManager.Instance;
        countdownManager = CountdownManager.Instance;
        currentPlayerData = currentPlayer.GetComponent<PlayerController>().playerData;
        currentPlayerStats = currentPlayer.GetComponent<PlayerController>().playerStats;

        if (currentPlayerStats != null) { 
            if(currentPlayerStats.powerUpMode != PowerUpMode.itemStats)
            {
                itemsPanel.SetActive(false);
            }
            else
            {
                itemsPanel.SetActive(true);
            }
        }        

        sceneReferences = SceneReferences.Instance;
        raceSettings = RaceSettings.Instance;

        StartCoroutine(UpdateSpeedCoroutine(10f)); // 10Hz
    }

    private void LateUpdate()
    {
        ShowRaceDataLines();
        ShowRaceResults();
        ShowPositionResult();
        ShowCountDown();
        ShowPauseMenu();
        ShowEnergy();
    }

    IEnumerator UpdateSpeedCoroutine(float Hertz)
    {
        float timeInterval = 1f / Hertz;

        var wait = new WaitForSeconds(timeInterval);

        Vector3 prevPosition = currentPlayer.transform.position;
        float prevSpeed = 0f;
        float speed = 0f;

        while (true)
        {
            Vector3 diffPosition = currentPlayer.transform.position - prevPosition;
            speed = UpdateSpeedometer(diffPosition, timeInterval, prevSpeed);
            float kmhSpeed = (int)(speed * 3.6f); // Convert m/s to km/h
            speedometerText.text = kmhSpeed.ToString();
            prevPosition = currentPlayer.transform.position;
            prevSpeed = speed;
            yield return wait;
        }
    }

    float UpdateSpeedometer(Vector3 diffPosition, float time, float prevSpeed)
    {
        if (canShowSpeedometer)
        {
            if (!speedometerPanel.activeInHierarchy)
            {
                speedometerPanel.SetActive(true);
            }

            float speed = 0;
            float realSpeed = diffPosition.magnitude / time;           
            speed = (realSpeed + prevSpeed) / 2;

            return speed;
        }
        else
        {
            speedometerPanel.SetActive(false);
            return 0;
        }
    }

    public void ShowPauseMenu()
    {
        if(canShowPauseMenu)
        {
            if (!pauseMenuPanel.activeInHierarchy)
            {
                pauseMenuPanel.SetActive(true);
            }
        }
        else
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    public void ShowEnergy()
    {
        if (canShowStats)
        {
            if(!statsPanel.activeInHierarchy)
            {
                statsPanel.SetActive(true);
            }

            if (currentPlayerStats != null)
            {
                float currentEnergyValue = currentPlayerStats.Energy;
                float clampedEnergyValue = currentEnergyValue / 100f;
                Vector2 anchorMax = energyBar.anchorMax;
                anchorMax.x = clampedEnergyValue;
                energyBar.anchorMax = anchorMax;
            }
            else
            {
                Debug.LogError("currentPlayerStats is null!");
            }
        }
        else
        {
            statsPanel.SetActive(false);
        }

        
    }

    void ShowRaceDataLines()
    {
        if (canShowRaceDataLines) {

            if (!raceDataPanel.activeInHierarchy)
            {
                raceDataPanel.SetActive(true);
                currentPositionPanel.SetActive(true);
            }

            PlayerRaceData currentPlayerRaceData = raceManager.GetRaceData().GetPlayerRaceDataByID(currentPlayerData.nameId);
            string positionLine = "Position: " + currentPlayerRaceData.position;
            string currentLap = "Lap" + currentPlayerRaceData.currentLap + "/" + raceManager.maxLaps;
            string currentTime = "Current Lap Time: " + currentPlayerRaceData.GetCurrentLapTime();
            string bestLapTime = "Best: " + currentPlayerRaceData.GetBestLapTime();


            string finalString = "";

            finalString += currentLap;
            finalString += "\n";
            finalString += currentTime;
            finalString += "\n";
            finalString += bestLapTime;

            raceDataText.text = finalString;

            string currentPosition = "--";
            Color textColor = Color.grey;

            if (currentPlayerRaceData.position != 0){
                currentPosition = currentPlayerRaceData.position.ToString();

                if (currentPlayerRaceData.position == 1) {
                    textColor = goldColor;
                }else if(currentPlayerRaceData.position == 2){
                    textColor = silverColor;
                }else if (currentPlayerRaceData.position == 3){
                    textColor = bronzeColor;
                }
            }

            currentPositionText.text = currentPosition;
            currentPositionText.color = textColor;
        }
        else
        {
            raceDataPanel.SetActive(false);
            currentPositionPanel.SetActive(false);
        }

        
    }

    void ShowRaceResults()
    {
        if (canShowResults) {
            if (!resultsPanel.activeInHierarchy)
            {
                resultsPanel.SetActive(true);
            }
        }
    }

    void ShowPositionResult()
    {
        if (canShowPositionResult)
        {
            if (!positionResultPanel.activeInHierarchy)
            {
                positionResultPanel.SetActive(true);
            }
        }
    }

    void ShowCountDown()
    {
        if (canShowCountdown) {
            if(countdownManager != null)
            {
                int count = countdownManager.count;
                if (count > 0) {
                    countdownText.text = count.ToString();
                }
                else
                {
                    countdownText.text = "GO";
                }
            }
            else
            {
                countdownText.text = "CountdownManager not found";
            }
            
        }
        else
        {
            countdownText.text = "";
        }
    }

    public void Finish()
    {
        finishLabel.SetActive(true);
    }

    public void SetCanShowResults(bool canShowResults)
    {
        this.canShowResults = canShowResults;

        if (this.canShowResults) {

            humanNameIdList = raceManager.GetAllPlayerInstances().ToArray().Where(player => player.GetComponent<PlayerController>().IsHuman()).Select(player => player.GetComponent<PlayerController>().playerData.nameId).ToList();

            RaceData raceData = raceManager.GetRaceData();

            UIListManager resultListManager = resultsPanelContent.GetComponent<UIListManager>();

            List<PlayerRaceData> finalPlayerRaceDataList = raceData.GetFinalPlayerRaceDataList();

            // Rows are built with the pre-race total already showing (points for this race are
            // awarded to raceSettings before results are shown - see RaceManager.OnRaceEnd - so
            // the post-race total minus this race's points recovers what the total was before
            // it). The "gained this race" cell's FINAL text is written immediately too -
            // AnimateResultsPoints only hides it via scale (which doesn't affect layout) and pops
            // it in. Building it blank and filling it in later, after the row's ContentSizeFitter
            // had already measured an empty string, was leaving the row too narrow to fit it.
            List<(GameObject row, int preTotal, int postTotal)> animatedRows = new List<(GameObject, int, int)>();

            for (int i = 0; i < finalPlayerRaceDataList.Count; i++)
            {
                int position = i + 1;
                string name = finalPlayerRaceDataList[i].playerData.displayName;
                string totalTime = finalPlayerRaceDataList[i].GetTotalTime();
                string bestTime = finalPlayerRaceDataList[i].GetBestLapTime();

                int racePoints = raceSettings != null ? raceSettings.GetPointsForPosition(position) : 0;
                int postTotal = raceSettings != null ? raceSettings.GetTotalPointsForPlayer(finalPlayerRaceDataList[i].playerData.nameId) : 0;
                int preTotal = postTotal - racePoints;

                // The total column is built with the POST-race value (the wider of the two, since
                // it can only be >= preTotal) so the row's ContentSizeFitter reserves enough width
                // for whatever the count-up animation will end on, then immediately rolled back
                // to the pre-race value to display before the animation starts.
                // NOTE: no "+" prefix - the '+' glyph renders as a solid colored blob instead of
                // text with this project's font asset (confirmed live: the same cell renders a
                // clean number without it), so the gained-points cell is just the bare number in
                // green instead.
                List<string> columns = new List<string>() { position.ToString(), name, totalTime, bestTime, postTotal.ToString(), racePoints.ToString() };

                GameObject row = humanNameIdList.Contains(finalPlayerRaceDataList[i].playerData.nameId)
                    ? resultListManager.AddRow(columns, highlightColor)
                    : resultListManager.AddRow(columns);

                // Force the row to size itself for this content right now, instead of waiting for
                // whichever frame Unity's own dirty-tracking would otherwise batch it to - building
                // it wide enough only once the total/gain text arrived later (rather than at
                // creation) left the row too narrow to fit them.
                LayoutRebuilder.ForceRebuildLayoutImmediate(row.GetComponent<RectTransform>());

                TMP_Text[] rowTexts = row.GetComponentsInChildren<TMP_Text>();
                if (rowTexts.Length >= 5)
                {
                    rowTexts[4].text = preTotal.ToString();
                }

                animatedRows.Add((row, preTotal, postTotal));
            }

            //resultString = GetRaceResultLines();
            canShowSpeedometer = false;
            canShowRaceDataLines = false;
            canShowStats = false;

            UpdateTrophyRecap();
            StartCoroutine(AnimateResultsPoints(animatedRows));

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(resultMenuSelectionStart);
        }
    }

    // Recaps which trophy stage this was, and shows/hides the green "continue" button - it only
    // makes sense while there is a next track to continue to. The always-present red "exit"
    // button is a plain Button wired to OnRaceExit() directly, so it needs no logic here.
    private void UpdateTrophyRecap()
    {
        bool trophyActive = raceSettings != null && raceSettings.IsTrophyActive();
        bool hasNextStage = trophyActive && !raceSettings.IsLastTrophyStage();

        if (trophyRecapPanel != null)
        {
            trophyRecapPanel.SetActive(trophyActive);
        }

        if (trophyActive && trophyRecapText != null)
        {
            trophyRecapText.text = $"{raceSettings.GetSelectedTrophyName()} - Stage {raceSettings.GetTrophyStageNumber()}/{raceSettings.GetTrophyStageCount()}";
        }

        if (continueButton != null)
        {
            continueButton.SetActive(hasNextStage);
        }
    }

    // Pops each row's gained-points cell in, then counts its total column up from the
    // pre-race to the post-race value - staggered per row so the whole list doesn't animate at
    // once. Unscaled time so it still plays if something has paused Time.timeScale.
    private IEnumerator AnimateResultsPoints(List<(GameObject row, int preTotal, int postTotal)> rows)
    {
        for (int i = 0; i < rows.Count; i++)
        {
            StartCoroutine(AnimateResultsRow(rows[i].row, rows[i].preTotal, rows[i].postTotal, i * resultsRowStagger));
        }
        yield return null;
    }

    private IEnumerator AnimateResultsRow(GameObject row, int preTotal, int postTotal, float delay)
    {
        if (row == null) yield break;

        TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();
        if (texts.Length < 6) yield break;

        TMP_Text totalText = texts[4];
        TMP_Text gainText = texts[5];

        // Text for both cells is already final (set in SetCanShowResults, before the row's
        // layout was sized) - the gain cell just needs hiding here, to fade in later.
        // NOTE: this fades alpha rather than popping in via RectTransform.localScale (the
        // obvious choice for a "pop") because animating a freshly-created TMP text's scale away
        // from and back to normal leaves its CanvasRenderer mesh stuck rendering as a solid
        // colored blob instead of glyphs - confirmed live, and not fixable by forcing a mesh
        // rebuild afterwards either (the very next scale change re-breaks it, so even doing that
        // once the scale animation is entirely over doesn't stick). Plain color/alpha changes
        // don't trigger it.
        Color visibleColor = gainedPointsColor;
        Color hiddenColor = new Color(visibleColor.r, visibleColor.g, visibleColor.b, 0f);
        gainText.color = hiddenColor;

        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        float t = 0f;
        while (t < pointsPopDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / pointsPopDuration);
            gainText.color = Color.Lerp(hiddenColor, visibleColor, p);
            yield return null;
        }
        gainText.color = visibleColor;

        t = 0f;
        while (t < totalCountDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / totalCountDuration);
            totalText.text = Mathf.RoundToInt(Mathf.Lerp(preTotal, postTotal, p)).ToString();
            yield return null;
        }
        totalText.text = postTotal.ToString();

        // The row was already sized to fit postTotal at creation (see SetCanShowResults), so this
        // is just a safety net in case anything nudged its layout dirty in the meantime.
        LayoutRebuilder.ForceRebuildLayoutImmediate(row.GetComponent<RectTransform>());
    }

    public void SetCanShowPositionResult(bool canShowPositionResults)
    {
        this.canShowPositionResult = canShowPositionResults;

        PlayerRaceData currentPlayerRaceData = raceManager.GetRaceData().GetFinalPlayerRaceDataByID(currentPlayerData.nameId);
        string positionString = "" + currentPlayerRaceData.position;

        Color resultPositionColor = Color.grey;

        if (currentPlayerRaceData.position == 1)
        {
            resultPositionColor = goldColor;
        }
        else if (currentPlayerRaceData.position == 2)
        {
            resultPositionColor = silverColor;
        }
        else if (currentPlayerRaceData.position == 3)
        {
            resultPositionColor = bronzeColor;
        }

        positionResultText.text = positionString;
        positionResultText.color = resultPositionColor;

        canShowSpeedometer = false;
        canShowRaceDataLines = false;
        canShowStats = false;
    }

    public void SetCanShowCountdown(bool canShowCountdown)
    {
        this.canShowCountdown = canShowCountdown;
        countdownText.gameObject.SetActive(canShowCountdown);
    }

    public void SetCanShowPauseMenu(bool canShowPauseMenu)
    {
        this.canShowPauseMenu = canShowPauseMenu;
        pauseMenuPanel.SetActive(canShowPauseMenu);
        if(canShowPauseMenu)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(pauseMenuSelectionStart);
        }
    }

    public void SetItemPanelActive(int panelIndex, bool active)
    {
        if (panelIndex < 0 || panelIndex >= itemPanelList.Length)
        {
            Debug.LogWarning("ItemPanel index not found");
            return;
        }

        Image imageComponent = itemPanelList[panelIndex].GetComponent<Image>();
        if (imageComponent != null)
        {
            if (active) {
                imageComponent.color = Color.white;
            }
            else
            {
                imageComponent.color = Color.gray;
            }            
        }
        else
        {
            Debug.LogWarning("No Image Component found for panel");
        }
    }

    public void SetItemPanelImage(int panelIndex, int spriteIndex)
    {
        if (panelIndex < 0 || panelIndex >= itemPanelImageList.Length)
        {
            Debug.LogWarning("PanelImage index not found");
            return;
        }

        if (spriteIndex < 0 || spriteIndex >= availableSprites.Count)
        {
            Debug.LogWarning("sprite index not valid");
            return;
        }

        Image imageComponent = itemPanelImageList[panelIndex].GetComponent<Image>();
        if (imageComponent != null)
        {
            imageComponent.sprite = availableSprites[spriteIndex];
        }
        else
        {
            Debug.LogWarning("Nessun componente Image trovato nel pannello.");
        }
    }

    public void OnPauseContinue()
    {
        if (raceManager != null) {
            raceManager.OnPauseExit();
        }
    }

    // Bound to the results panel's green "Continue" button, which UpdateTrophyRecap only shows
    // while there's a next trophy track to go to. The "else" below is just a safety net for
    // that button somehow being clicked outside of that state.
    public void OnResultsContinue()
    {
        if (raceSettings != null && raceSettings.IsTrophyActive() && !raceSettings.IsLastTrophyStage())
        {
            if (raceManager != null)
            {
                raceManager.ExitRace();
            }

            raceSettings.AdvanceTrophyToNextTrack();
            LoadScene(raceSettings.GetSelectedRaceTrack());
            return;
        }

        OnRaceExit();
    }

    public void OnRaceExit()
    {
        if (raceManager != null) {
            raceManager.ExitRace();
        }

        if (raceSettings != null)
        {
            raceSettings.ResetSettings();
        }
        else
        {
            Debug.LogError("RaceSettings instance is null!");
        }

        if (sceneReferences != null)
        {
            LoadScene(sceneReferences.startScene);
        }
        else
        {
            Debug.LogError("sceneReferences instance is null!!");
        }

    }

    private void LoadScene(string sceneName)
    {
        if (LoadingScreenManager.Instance != null)
        {
            LoadingScreenManager.Instance.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("[RaceGUI] LoadingScreenManager instance not found, loading without a loading screen.");
            SceneManager.LoadScene(sceneName);
        }
    }
}
