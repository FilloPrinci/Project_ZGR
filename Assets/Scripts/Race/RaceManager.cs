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
    public static RaceManager Instance { get; private set; }

    public GameObject mainCamera;
    public RaceMode mode;
    public List<PlayerData> playerDataList;
    public GameObject veichlePivotPrefab;
    public GameObject playerPrefab;
    public List<GameObject> checkPointList;
    public List<Transform> spawnPointList;
    public int maxLaps;
    public GameObject racePlayerCanvasPrefab;

    private int currentLap;
    private RacePhase currentRacePhase;
    private RacePhaseEvent lastRacePhaseEvent;
    private bool isManagerReady = false;
    private List<GameObject> playerInstanceList;
    private List<int> sectorList;
    private RaceData raceData;
    private List<GameObject> racePlayerCanvasInstanceList;

    public RaceData GetRaceData()
    {
        return raceData;
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
                    playerRaceDataList.Add(new PlayerRaceData(playerDataList[i], 0, "", 0, 0, 0, 0));

                }

                raceData = new RaceData(playerRaceDataList);

                switch (mode)
                {
                    case RaceMode.Test:
                        playerInstanceList = new List<GameObject>();
                        GameObject testPlayer = Instantiate(playerPrefab, spawnPointList[0].position, spawnPointList[0].rotation);
                        GameObject testVeichlePivot = Instantiate(veichlePivotPrefab);

                        GameObject racePlayerCanvasInstance = Instantiate(racePlayerCanvasPrefab);
                        racePlayerCanvasInstance.GetComponent<RaceGUI>().currentPlayer = testPlayer;

                        testPlayer.GetComponent<PlayerController>().veichlePivot = testVeichlePivot.GetComponent<Transform>();
                        testPlayer.GetComponent<PlayerController>().playerData = playerDataList[0];
                        testPlayer.GetComponent<FeedBackManager>().playerCamera = mainCamera;
                        mainCamera.GetComponent<CameraController>().cameraDesiredPosition = testVeichlePivot.transform.Find("CameraPositionTarget");
                        playerInstanceList.Add(testPlayer);
                        lastRacePhaseEvent = RacePhaseEvent.RaceStart;
                        break;
                    case RaceMode.TimeTrial:
                        break;
                    case RaceMode.RaceSingleplayer:
                        break;
                    case RaceMode.RaceMultiplayer:
                        break;
                }
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
        Debug.Log($"[RaceManager] [FixedUpdate]: currentRacePhase : {currentRacePhase}");
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
                }
            }

            raceData.playerRaceDataList[playerDataToUpdateIndex].nextCheckpointIndex = raceData.playerRaceDataList[playerDataToUpdateIndex].currentSectorIndex;
        }
        else
        {
            Debug.Log($"[RaceManager] [OnCheckpoint({playerID})]: Checkpoint is NOT valid");
        }

    }

    /*
    public void UpdatePlayerData(string playerID, PlayerRaceData updatedRaceData)
    {
        int playerDataToUpdateIndex = raceData.playerRaceDataList.FindIndex(p => p.playerData.getID() == playerID);
        if (playerDataToUpdateIndex != -1)
        {
            raceData.playerRaceDataList[playerDataToUpdateIndex] = updatedRaceData;
        }

    }
    */

    public void TriggerRaceEvent(RacePhaseEvent newRacePhaseEvent)
    {
        lastRacePhaseEvent = newRacePhaseEvent;
    }
}
