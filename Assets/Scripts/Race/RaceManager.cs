using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public static RaceManager Instance { get; private set; }

    public Camera mainCamera;
    public RaceMode mode;
    public List<PlayerData> playerDataList;
    public GameObject playerPrefab;
    public List<GameObject> checkPointList;
    public List<Transform> spawnPointList;
    public int maxLaps;
    public GameObject presentationManager;

    private int currentLap;
    private RacePhase currentRacePhase;
    private RacePhaseEvent lastRacePhaseEvent;
    private List<GameObject> playerInstanceList;
    private List<int> sectorList;
    private RaceData raceData;

    public RaceData GetRaceData()
    {
        return raceData;
    }

    public RacePhase GetCurrentRacePhase()
    {
        return currentRacePhase;   
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Duplicate RaceManager detected. Destroying extra instance.");
            Destroy(gameObject); // Assicura che ci sia solo un RaceManager
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentLap = 0;

        if (playerDataList.Count != 0)
        {
            if (checkPointList == null || checkPointList.Count == 0 )
            {
                Debug.Log($"[RaceManager] : No Checkpoint has been set");
            }
            else
            {
                // Generate Sectors
                sectorList = new List<int>();
                for (int i = 0; i<checkPointList.Count; i++)
                {
                    sectorList.Add(i);
                }

                // get raceData ready

                List<PlayerRaceData> playerRaceDataList = new List<PlayerRaceData>();

                for(int i = 0; i < playerDataList.Count; i++)
                {
                    playerRaceDataList.Add(new PlayerRaceData(playerDataList[i], 0, "", 0, 0, 0, 1));

                }

                raceData = new RaceData(playerRaceDataList);

                switch (mode)
                {
                    case RaceMode.Test:

                        playerInstanceList = new List<GameObject>();

                        GameObject newPlayer = InstantiatePlayer(playerDataList[0], spawnPointList[0]);

                        playerInstanceList.Add(newPlayer);
                        lastRacePhaseEvent = RacePhaseEvent.Start;
                        break;
                    case RaceMode.TimeTrial:
                        break;
                    case RaceMode.RaceSingleplayer:
                        break;
                    case RaceMode.RaceMultiplayer:
                        break;
                }
            }
            TriggerRaceEvent(RacePhaseEvent.Start);
        }
    }
    
    public GameObject InstantiatePlayer(PlayerData playerData, Transform spawnPoint) {

        playerPrefab.GetComponent<PlayerStructure>().data = playerData;
        GameObject newPlayer = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        return newPlayer;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        switch (currentRacePhase) {
            case RacePhase.Presentation:
                // manage presentation
                break;
            case RacePhase.CountDown:
                // manage countdown
                break;
            case RacePhase.Race:
                // Manage the race
                ManageRace(mode);
        
                break;
            case RacePhase.Results:
                // Show race results
                break;
        }
        Debug.Log($"[RaceManager] [FixedUpdate]: currentRacePhase : {currentRacePhase}");
    }

    private void OnTrackPresentationStart() {
        if (presentationManager != null)
        {
            presentationManager.GetComponent<PresentationManager>().SetCamera(mainCamera);
        }
        else
        {
            mainCamera.transform.position = Vector3.zero;
        }
    }


    private void OnCountDownStart() { 

        mainCamera.gameObject.SetActive(false);

        // switch to players cameras
        foreach (GameObject playerInstnce in playerInstanceList)
        {
            PlayerStructure instanceStructure = playerInstnce.GetComponent<PlayerStructure>();
            if (instanceStructure.data.playerInputIndex != InputIndex.CPU) {
                if (mode != RaceMode.RaceMultiplayer) {
                    instanceStructure.ActivatePlayerCamera(CameraMode.SinglePlayer);
                }
                else
                {
                    Debug.Log("Multiplayer camera not implemented yet");
                }
                
            }
        }

        // start count down
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

        RefreshPlayerRaceDataDistances();
        raceData.RefreshPositions();
    }

    public GameObject GetPlayerInstanceFromID(string id)
    {
        GameObject playerInstance = playerInstanceList.Find(p => p.GetComponent<PlayerController>().playerData.getID() == id);
        return playerInstance;
    }

    public void RefreshPlayerRaceDataDistances()
    {
        foreach(PlayerRaceData playerRaceData in raceData.playerRaceDataList)
        {
            GameObject playerInstance = GetPlayerInstanceFromID(playerRaceData.playerData.getID());

            // calculate new checkpoint distance
            Vector3 newDistanceVector = playerInstance.transform.position - checkPointList[playerRaceData.nextCheckpointIndex].transform.position;
            float distance = newDistanceVector.magnitude;

            playerRaceData.currentCheckpointDistance = distance;
        }
    }

    public void OnCheckpoint(string playerID, GameObject checkPointGameObject)
    {
        int playerDataToUpdateIndex = raceData.playerRaceDataList.FindIndex(p => p.playerData.getID() == playerID);
        if (checkPointGameObject.transform.parent?.gameObject == checkPointList[raceData.playerRaceDataList[playerDataToUpdateIndex].nextCheckpointIndex])
        {
            Debug.Log($"[RaceManager] [OnCheckpoint({playerID})]: Checkpoint valid");

            raceData.playerRaceDataList[playerDataToUpdateIndex].currentSectorIndex++;
            if (raceData.playerRaceDataList[playerDataToUpdateIndex].currentSectorIndex > checkPointList.Count - 1)
            {
                raceData.playerRaceDataList[playerDataToUpdateIndex].currentSectorIndex = 0;
                raceData.playerRaceDataList[playerDataToUpdateIndex].currentLap++;
                if (raceData.playerRaceDataList[playerDataToUpdateIndex].currentLap > maxLaps)
                {
                    // finish race
                    if (isRaceEnded())
                    {
                        TriggerRaceEvent(RacePhaseEvent.RaceEnd);
                    }
                }
            }

            raceData.playerRaceDataList[playerDataToUpdateIndex].nextCheckpointIndex = raceData.playerRaceDataList[playerDataToUpdateIndex].currentSectorIndex;
        }
        else
        {
            Debug.Log($"[RaceManager] [OnCheckpoint({playerID})]: Checkpoint is NOT valid");
        }

    }

    bool isRaceEnded()
    {

        // Race is over only when all the players ended the race
        bool isRaceEnded = true;
        foreach (PlayerRaceData playerRasceData in raceData.playerRaceDataList)
        {
            isRaceEnded = isRaceEnded && playerRasceData.RaceCompleted(maxLaps);
        }
        return isRaceEnded;
    }

    public void TriggerRaceEvent(RacePhaseEvent newRacePhaseEvent)
    {
        switch (newRacePhaseEvent)
        {
            case RacePhaseEvent.Start:
                // Start presentation
                currentRacePhase = RacePhase.Presentation;
                OnTrackPresentationStart();
                break;
            case RacePhaseEvent.PresentationEnd:
                // Start countdown
                currentRacePhase = RacePhase.CountDown;
                OnCountDownStart();
                break;
            case RacePhaseEvent.RaceStart:
                // Start Race
                currentRacePhase = RacePhase.Race;
                break;
            case RacePhaseEvent.RaceEnd:
                currentRacePhase = RacePhase.Results;
                break;
        }
    }

    public void OnSkip()
    {
        if (currentRacePhase == RacePhase.Presentation) {
            Debug.Log("[RaceManager] : Skip Presentation");
            TriggerRaceEvent(RacePhaseEvent.PresentationEnd);
        }
    }
}
