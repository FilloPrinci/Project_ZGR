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
    public GameObject finishLabel;
    private RaceManager raceManager;
    private PlayerData currentPlayerData;

    private bool canShowResults = false;
    private bool canShowCountdown = false;

    private string resultString;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        raceManager = RaceManager.Instance;
        currentPlayerData = currentPlayer.GetComponent<PlayerController>().playerData;
    }

    // Update is called once per frame
    void Update()
    {

        
    }

    private void FixedUpdate()
    {
        ShowRaceDataLines();
        ShowRaceResults();
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
