using NUnit.Framework.Constraints;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class RaceGUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject resultsPanel;
    public GameObject pauseMenuPanel;
    public GameObject raceDataPanel;
    public GameObject speedometerPanel;
    public GameObject statsPanel;
    public GameObject positionResultPanel;

    [Header("Texts")]
    public TextMeshProUGUI raceDataText;
    public TextMeshProUGUI resultsDataText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI speedometerText;
    public TextMeshProUGUI positionResultText;

    [Header("GameObjects")]
    public GameObject currentPlayer;
    public GameObject pauseMenuSelectionStart;
    public GameObject resultMenuSelectionStart;

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

    private bool canShowRaceDataLines = true;
    private bool canShowSpeedometer = true;
    private bool canShowStats = true;
    private bool canShowResults = false;
    private bool canShowPositionResult = false;
    private bool canShowCountdown = false;
    private bool canShowPauseMenu = false;

    private string resultString;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        raceManager = RaceManager.Instance;
        countdownManager = CountdownManager.Instance;
        currentPlayerData = currentPlayer.GetComponent<PlayerController>().playerData;
        currentPlayerStats = currentPlayer.GetComponent<PlayerController>().playerStats;
        sceneReferences = SceneReferences.Instance;
    }

    // Update is called once per frame
    void Update()
    {

        
    }

    private void FixedUpdate()
    {
        ShowRaceDataLines();
        ShowRaceResults();
        ShowPositionResult();
        ShowCountDown();
        ShowPauseMenu();
        UpdateSpeedometer(Time.fixedDeltaTime);
        ShowEnergy();
    }

    void UpdateSpeedometer(float time)
    {
        if (canShowSpeedometer)
        {
            if (!speedometerPanel.activeInHierarchy)
            {
                speedometerPanel.SetActive(true);
            }

            int speed = 0;
            float? realSpeed = currentPlayer.GetComponent<PlayerController>().GetCurrentSpeed();
            realSpeed *= 3.6f; // Convert m/s to km/h
            if (realSpeed != null)
            {
                speed = (int)realSpeed;
            }

            speedometerText.text = speed.ToString();
        }
        else
        {
            speedometerPanel.SetActive(false);
            return;
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
            }

            PlayerRaceData currentPlayerRaceData = raceManager.GetRaceData().GetPlayerRaceDataByID(currentPlayerData.name);
            string positionLine = "Position: " + currentPlayerRaceData.position;
            string currentLap = "Lap" + currentPlayerRaceData.currentLap + "/" + raceManager.maxLaps;
            string currentTime = "Current Lap Time: " + currentPlayerRaceData.GetCurrentLapTime();
            string bestLapTime = "Best: " + currentPlayerRaceData.GetBestLapTime();


            string finalString = "";
            /*
            List<string> lines = raceManager.GetRaceData().GetRaceDataAsLines();
            foreach (string line in lines) { 
                finalString += line;
                finalString += "\n";
            }*/

            finalString += positionLine;
            finalString += "\n";
            finalString += currentLap;
            finalString += "\n";
            finalString += currentTime;
            finalString += "\n";
            finalString += bestLapTime;

            raceDataText.text = finalString;
        }
        else
        {
            raceDataPanel.SetActive(false);
        }

        
    }

    void ShowRaceResults()
    {
        if (canShowResults) {
            if (!resultsPanel.activeInHierarchy)
            {
                resultsPanel.SetActive(true);
            }

            resultsDataText.text = resultString;

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
            resultString = GetRaceResultLines();
            canShowSpeedometer = false;
            canShowRaceDataLines = false;
            canShowStats = false;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(resultMenuSelectionStart);
        }
    }

    public void SetCanShowPositionResult(bool canShowPositionResults)
    {
        this.canShowPositionResult = canShowPositionResults;

        PlayerRaceData currentPlayerRaceData = raceManager.GetRaceData().GetFinalPlayerRaceDataByID(currentPlayerData.name);
        string positionString = "" + currentPlayerRaceData.position;

        positionResultText.text = positionString;
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

    string GetRaceResultLines()
    {
        List<string> playerLineList = new List<string>();

        RaceData raceData = raceManager.GetRaceData();

        string playerLine = $"P \t Name \t Time \t Best Time";
        playerLineList.Add(playerLine);

        List<PlayerRaceData> finalPlayerRaceDataList = raceData.GetFinalPlayerRaceDataList();

        for (int i = 0; i < finalPlayerRaceDataList.Count; i++)
        {
            int position = i + 1;
            string name = finalPlayerRaceDataList[i].playerData.name;
            string totalTime = finalPlayerRaceDataList[i].GetTotalTime();
            string bestTime = finalPlayerRaceDataList[i].GetBestLapTime();

            playerLine = $"{position}\t{name}\t{totalTime}\t{bestTime}";
            playerLineList.Add(playerLine);
        }

        string resultLinesString = "";

        for (int i = 0; i < playerLineList.Count; i++)
        {
            resultLinesString += playerLineList[i] + "\n";
        }

        return resultLinesString;
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

    public void OnRaceExit()
    {
        if (raceManager != null) {
            raceManager.ExitRace();
        }

        if(sceneReferences.startScene != null)
        {
            SceneManager.LoadScene(sceneReferences.startScene);
        }
        else
        {
            Debug.LogError("Start scene not set in SceneReferences!");
        }
        
    }
}
