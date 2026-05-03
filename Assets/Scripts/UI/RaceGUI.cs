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

            for (int i = 0; i < finalPlayerRaceDataList.Count; i++)
            {
                int position = i + 1;
                string name = finalPlayerRaceDataList[i].playerData.displayName;
                string totalTime = finalPlayerRaceDataList[i].GetTotalTime();
                string bestTime = finalPlayerRaceDataList[i].GetBestLapTime();

                if (humanNameIdList.Contains(finalPlayerRaceDataList[i].playerData.nameId)){
                    resultListManager.AddRow(new List<string>() { position.ToString(), name, totalTime, bestTime }, highlightColor);
                }
                else
                {
                    resultListManager.AddRow(new List<string>() { position.ToString(), name, totalTime, bestTime });
                }

                
            }

            //resultString = GetRaceResultLines();
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
            SceneManager.LoadScene(sceneReferences.startScene);
        }
        else
        {
            Debug.LogError("sceneReferences instance is null!!");
        }
        
    }
}
