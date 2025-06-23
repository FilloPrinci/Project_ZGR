using NUnit.Framework.Constraints;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RaceGUI : MonoBehaviour
{
    public TextMeshProUGUI raceDataText;
    public GameObject currentPlayer;
    public GameObject resultsPanel;
    public TextMeshProUGUI resultsDataText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI speedometerText;
    public GameObject finishLabel;
    public RectTransform energyBar;

    private RaceManager raceManager;
    private CountdownManager countdownManager;
    private PlayerData currentPlayerData;
    private PlayerStats currentPlayerStats;

    private bool canShowResults = false;
    private bool canShowCountdown = false;

    private string resultString;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        raceManager = RaceManager.Instance;
        countdownManager = CountdownManager.Instance;
        currentPlayerData = currentPlayer.GetComponent<PlayerController>().playerData;
        currentPlayerStats = currentPlayer.GetComponent<PlayerController>().playerStats;
    }

    // Update is called once per frame
    void Update()
    {

        
    }

    private void FixedUpdate()
    {
        ShowRaceDataLines();
        ShowRaceResults();
        ShowCountDown();
        UpdateSpeedometer();
        ShowEnergy();
    }

    void UpdateSpeedometer()
    {
        int speed = 0;
        float? realSpeed = currentPlayer.GetComponent<Speedometer>().speedKmh;
        if (realSpeed != null) {
            speed = (int)realSpeed;
        }
        
        speedometerText.text = speed.ToString();
    }

    public void ShowEnergy()
    {
        if (currentPlayerStats != null) {
            int currentEnergyValue = currentPlayerStats.Energy;
            float clampedEnergyValue = (float)currentEnergyValue / 100f;
            Vector2 anchorMax = energyBar.anchorMax;
            anchorMax.x = clampedEnergyValue;
            energyBar.anchorMax = anchorMax;
        }
        else
        {
            Debug.LogError("currentPlayerStats is null!");
        }
    }

    void ShowRaceDataLines()
    {
        PlayerRaceData currentPlayerRaceData = raceManager.GetRaceData().GetPlayerRaceDataByID(currentPlayerData.name);
        string positionLine = "Position: " + currentPlayerRaceData.position;
        string currentLap = "Lap" + currentPlayerRaceData.currentLap + "/" + raceManager.maxLaps;
        string raceStatus = "RaceStatus: " + raceManager.GetCurrentRacePhase().ToString();

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
        finalString += raceStatus;

        raceDataText.text = finalString;
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
        }
    }

    public void SetCanShowCountdown(bool canShowCountdown)
    {
        this.canShowCountdown = canShowCountdown;
        countdownText.gameObject.SetActive(canShowCountdown);
    }

    string GetRaceResultLines()
    {
        List<string> playerLineList = new List<string>();

        RaceData raceData = raceManager.GetRaceData();

        raceData.playerRaceDataList.Sort((a, b) => a.position.CompareTo(b.position));
        for (int i = 0; i < raceData.playerRaceDataList.Count; i++)
        {
            int position = raceData.playerRaceDataList[i].position;
            string name = raceData.playerRaceDataList[i].playerData.name;

            string playerLine = $"{position}\t{name}";
            playerLineList.Add(playerLine);
        }

        string resultLinesString = "";

        for (int i = 0; i < playerLineList.Count; i++)
        {
            resultLinesString += playerLineList[i] + "\n";
        }

        return resultLinesString;
    }
}
