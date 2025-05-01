using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum RaceMode
{
    Test,
    TimeTrial,
    RaceSingleplayer,
    RaceMultiplayer
}

public enum RacePhase 
{
    None,
    Presentation,
    CountDown,
    Race,
    Results
}

public enum RacePhaseEvent
{
    Start,
    PresentationEnd,
    RaceStart,
    RaceEnd
}

public class RaceManager : MonoBehaviour
{
    public GameObject mainCamera;
    public RaceMode mode;
    public List<PlayerData> playerDataList;
    public GameObject veichlePivotPrefab;
    public GameObject playerPrefab;
    public List<GameObject> checkPointList;
    public List<Transform> spawnPointList;
    public int maxLaps;

    private int currentLap;
    private RacePhase currentRacePhase;
    private RacePhaseEvent lastRacePhaseEvent;
    private bool isManagerReady = false;
    private List<GameObject> playerInstanceList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentLap = 0;

        if (playerDataList.Count != 0)
        {
            switch (mode)
            {
                case RaceMode.Test:
                    playerInstanceList = new List<GameObject>();
                    GameObject testPlayer = Instantiate(playerPrefab, spawnPointList[0].position, spawnPointList[0].rotation);
                    GameObject testVeichlePivot = Instantiate(veichlePivotPrefab);
                    testPlayer.GetComponent<PlayerController>().veichlePivot = testVeichlePivot.GetComponent<Transform>();
                    testPlayer.GetComponent<PlayerController>().veichlePrefab = playerDataList[0].playerVeichle;
                    testPlayer.GetComponent<FeedBackManager>().playerCamera = mainCamera;
                    mainCamera.GetComponent<CameraController>().cameraDesiredPosition = testVeichlePivot.transform.Find("CameraPositionTarget");
                    playerInstanceList.Add(testPlayer);
                    
                    break;
                case RaceMode.TimeTrial:
                    break;
                case RaceMode.RaceSingleplayer:
                    break;
                case RaceMode.RaceMultiplayer:
                    break;
            }

            isManagerReady = true;
        }
        else { 
            
        }

        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isManagerReady)
        {
            ManageRacePhase();

            switch (currentRacePhase) {
                case RacePhase.Presentation:
                    // Do the presentation
                    break;
                case RacePhase.CountDown:
                    // Do the countdown
                    break;
                case RacePhase.Race:
                    // Manage the race
                    ManageRace(mode);
                    break;
                case RacePhase.Results:
                    // Show race results
                    break;
            }
        }
    }

    private void ManageRace(RaceMode raceMode)
    {
        switch (raceMode)
        {
            case RaceMode.Test:

                break;
            case RaceMode.TimeTrial:
                break;
            case RaceMode.RaceSingleplayer:
                break;
            case RaceMode.RaceMultiplayer:
                break;
        }
    }

    private void ManageRacePhase()
    {
        switch (lastRacePhaseEvent)
        {
            case RacePhaseEvent.Start:
                currentRacePhase = RacePhase.Presentation;
                break;
            case RacePhaseEvent.PresentationEnd:
                currentRacePhase = RacePhase.CountDown;
                break;
            case RacePhaseEvent.RaceStart:
                currentRacePhase = RacePhase.Race;
                break;
            case RacePhaseEvent.RaceEnd:
                currentRacePhase = RacePhase.Results;
                break;
        }
    }

    public void TriggerRaceEvent(RacePhaseEvent newRacePhaseEvent)
    {
        lastRacePhaseEvent = newRacePhaseEvent;
    }
}
