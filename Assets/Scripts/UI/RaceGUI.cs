using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RaceGUI : MonoBehaviour
{
    public TextMeshProUGUI raceDataText;
    public GameObject currentPlayer;
    private RaceManager raceManager;
    private PlayerData currentPlayerData;
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
    }

    void ShowRaceDataLines()
    {
        PlayerRaceData currentPlayerRaceData = raceManager.GetRaceData().GetPlayerRaceDataByID(currentPlayerData.getID());
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
}
