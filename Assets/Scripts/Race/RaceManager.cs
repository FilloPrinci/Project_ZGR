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
   
    public bool generateCPUPlayers = true;
    public int cpuPlayersAmount = 0; // Number of CPU players to generate if generateCPUPlayers is true
    public GameObject playerPrefab;
    public List<GameObject> checkPointList;
    public List<Transform> spawnPointList;
    
    public GameObject presentationManager;

    [Header("Default parameters")]
    public RaceMode mode;
    public List<PlayerData> playerDataList;
    public int maxLaps;
    public  List<GameObject> avaiableVeichleList;

    private int currentLap;
    private RacePhase currentRacePhase;
    private RacePhaseEvent lastRacePhaseEvent;
    private List<GameObject> playerInstanceList;
    private List<int> sectorList;
    private RaceData raceData;
    

    private bool isPaused = false;

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

        if (generateCPUPlayers) { 
            Debug.Log($"[RaceManager] : Generating {cpuPlayersAmount} CPU Players");
            if (cpuPlayersAmount == 0)
            {
                cpuPlayersAmount = spawnPointList.Count - playerDataList.Count;
                Debug.Log($"[RaceManager] : CPU Players amount set to {cpuPlayersAmount} based on spawnPointList");
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentLap = 0;

        LoadRaceSettings();

        // Generating CPU Players if needed
        if (generateCPUPlayers)
        {
            for (int i = 0; i < cpuPlayersAmount; i++)
            {
                if(CPUInputHandlerManager.Instance.cpuPlayerAmount > i)
                {
                    PlayerData cpuPlayerData = new PlayerData("CPU" + (i + 1), avaiableVeichleList[0], InputIndex.CPU);
                    cpuPlayerData.SetPlayerIndex(i);
                    playerDataList.Add(cpuPlayerData);
                }
            }
        }

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
                playerInstanceList = new List<GameObject>();

                switch (mode)
                {
                    case RaceMode.Test:


                        GameObject newTestPlayer = InstantiatePlayer(playerDataList[0], spawnPointList[0]);

                        playerInstanceList.Add(newTestPlayer);
                        lastRacePhaseEvent = RacePhaseEvent.Start;
                        break;
                    case RaceMode.TimeTrial:

                        GameObject newTimeTrialPlayer = InstantiatePlayer(playerDataList[0], spawnPointList[0]);

                        playerInstanceList.Add(newTimeTrialPlayer);

                        lastRacePhaseEvent = RacePhaseEvent.Start;
                        break;
                    case RaceMode.RaceSingleplayer:

                        for (int i = 0; i < playerDataList.Count; i++)
                        {
                            GameObject newPlayer = InstantiatePlayer(playerDataList[i], spawnPointList[i]);

                            playerInstanceList.Add(newPlayer);
                        }

                        lastRacePhaseEvent = RacePhaseEvent.Start;

                        break;
                    case RaceMode.RaceMultiplayer:

                        for (int i = 0; i < playerDataList.Count; i++)
                        {
                            GameObject newPlayer = InstantiatePlayer(playerDataList[i], spawnPointList[i]);

                            playerInstanceList.Add(newPlayer);
                        }

                        
                        lastRacePhaseEvent = RacePhaseEvent.Start;
                        break;
                }
            }
            TriggerRaceEvent(RacePhaseEvent.Start);
        }
    }
    
    public void LoadRaceSettings()
    {
        RaceSettings currentRaceSettings = RaceSettings.Instance;
        if (currentRaceSettings == null) {
            Debug.Log($"[RaceManager] : No RaceSettings Instance found, using editor settings");
        }
        else
        {
            Debug.Log($"[RaceManager] : RaceSettings Instance found, using player configured settings");
            maxLaps = currentRaceSettings.laps;
            playerDataList = currentRaceSettings.GetAllPlayerDataList();
            mode = currentRaceSettings.GetSelectedRaceMode();
            avaiableVeichleList = currentRaceSettings.veichlePrefabList;
        }

    }

    public GameObject InstantiatePlayer(PlayerData playerData, Transform spawnPoint) {

        Debug.Log($"[RaceManager] : Instantiating player {playerData.name} at position {spawnPoint.position}");

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
    }

    private void OnTrackPresentationStart() {
        if (presentationManager != null)
        {
            presentationManager.GetComponent<PresentationManager>().SetCamera(mainCamera);
            presentationManager.GetComponent<PresentationManager>().OnStartPresentation();
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
                    if (playerInstanceList.Count == 2)
                    {
                        instanceStructure.ActivatePlayerCamera(CameraMode.MultiPlayer2);
                    }else if (playerInstanceList.Count == 3)
                    {
                        instanceStructure.ActivatePlayerCamera(CameraMode.MultiPlayer3);
                    }else if(playerInstanceList.Count == 4)
                    {
                        instanceStructure.ActivatePlayerCamera(CameraMode.MultiPlayer4);
                    }
                }
            }
        }

        CountdownManager countdownManager = CountdownManager.Instance;

        if (countdownManager != null) {
            // start count down
            ShowCountdownForAllPlayers();
            countdownManager.StartCountdown();
        }
        else
        {
            // Skipping the countdown 
            TriggerRaceEvent(RacePhaseEvent.RaceStart);
        }

    }

    private void OnRaceStart()
    {
        HideCountdownForAllPlayers();
        raceData.StartRace();
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
        raceData.UpdatePlayerRaceDataTimings();
    }

    public GameObject GetPlayerInstanceFromID(string id)
    {
        GameObject playerInstance = playerInstanceList[0];

        for (int i = 0; i< playerInstanceList.Count; i++)
        {
            string playerId = playerInstanceList[i].GetComponent<PlayerStructure>().data.name;
            if (playerId == id)
            {
                playerInstance = playerInstanceList[i];
            }
        }

        return playerInstance;
    }

    public void RefreshPlayerRaceDataDistances()
    {
        foreach(PlayerRaceData playerRaceData in raceData.playerRaceDataList)
        {
            GameObject playerInstance = GetPlayerInstanceFromID(playerRaceData.playerData.name);

            // calculate new checkpoint distance
            Vector3 newDistanceVector = playerInstance.transform.position - checkPointList[playerRaceData.nextCheckpointIndex].transform.position;
            float distance = newDistanceVector.magnitude;

            playerRaceData.currentCheckpointDistance = distance;
        }
    }

    public void OnCheckpoint(string playerID, GameObject checkPointGameObject)
    {
        int playerDataToUpdateIndex = raceData.playerRaceDataList.FindIndex(p => p.playerData.name == playerID);
        if (checkPointGameObject.transform.parent?.gameObject == checkPointList[raceData.playerRaceDataList[playerDataToUpdateIndex].nextCheckpointIndex])
        {
            raceData.playerRaceDataList[playerDataToUpdateIndex].currentSectorIndex++;

            // check if player has completed the lap
            if (raceData.playerRaceDataList[playerDataToUpdateIndex].currentSectorIndex > checkPointList.Count - 1)
            {
                // player has completed the lap

                raceData.SetLapTimeForPlayer(playerDataToUpdateIndex);


                raceData.playerRaceDataList[playerDataToUpdateIndex].currentSectorIndex = 0;
                raceData.playerRaceDataList[playerDataToUpdateIndex].currentLap++;
                if (raceData.playerRaceDataList[playerDataToUpdateIndex].currentLap > maxLaps)
                {
                    // player has finished the race

                    GameObject playerInstance = GetPlayerInstanceFromPlayerRaceData(raceData.playerRaceDataList[playerDataToUpdateIndex]);
                    RaceGUI playerRaceGUI = GetRaceGUIFromPlayerInstance(playerInstance);
                    if(playerRaceGUI != null)
                        playerRaceGUI.Finish();

                    // finish race
                    if (isRaceEnded())
                    {
                        TriggerRaceEvent(RacePhaseEvent.RaceEnd);
                    }
                }
            }

            raceData.playerRaceDataList[playerDataToUpdateIndex].nextCheckpointIndex = raceData.playerRaceDataList[playerDataToUpdateIndex].currentSectorIndex;
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
                presentationManager.GetComponent<PresentationManager>().OnEndPresentation();
                currentRacePhase = RacePhase.CountDown;
                OnCountDownStart();
                break;
            case RacePhaseEvent.RaceStart:
                // Start Race
                currentRacePhase = RacePhase.Race;
                OnRaceStart();
                break;
            case RacePhaseEvent.RaceEnd:
                currentRacePhase = RacePhase.Results;

                raceData.playerRaceDataList.Sort((a, b) => a.position.CompareTo(b.position));

                Debug.Log("the winner is : " + raceData.playerRaceDataList[0].playerData.name + " (player index : " + (int)raceData.playerRaceDataList[0].playerData.playerInputIndex  + " )");

                ShowResultForPlayer((int)raceData.playerRaceDataList[0].playerData.playerInputIndex);

                // finish race for all playuers
                foreach (GameObject playerInstance in playerInstanceList)
                {
                    if(playerInstance.GetComponent<PlayerStructure>().data.playerInputIndex != InputIndex.CPU)
                    {
                        PlayerStructure playerStructure = playerInstance.GetComponent<PlayerStructure>();
                        playerStructure.OnRaceEndPhase((int)raceData.playerRaceDataList[0].playerData.playerInputIndex);
                    }
                }
                break;
        }
    }

    public void OnPauseEnter(int playerIndex)
    {
        ShowPauseMenuForInputPlayer(playerIndex);
        Time.timeScale = 0f;
    }

    public void OnPauseExit()
    {
        Time.timeScale = 1f;
        HidePauseMenuForAllPlayers();
    }

    public void OnSkip(int playerIndex)
    {
        Debug.Log("[RaceManager] : Player" + (playerIndex + 1) + " pressed SKIP button");

        if (currentRacePhase == RacePhase.Presentation)
        {
            Debug.Log("[RaceManager] : Skip Presentation");
            TriggerRaceEvent(RacePhaseEvent.PresentationEnd);
        }
        else if (currentRacePhase == RacePhase.Race)
        {
            Debug.Log("[RaceManager] : Pause race");
            isPaused = !isPaused;

            if (isPaused) {
                OnPauseEnter(playerIndex);
            }
            else
            {
                OnPauseExit();
            }
        }
        else if (currentRacePhase == RacePhase.Results)
        {
            Debug.Log("[RaceManager] : Exit from race");
        }
    }

    public void ShowPauseMenuForInputPlayer(int playerIndex)
    {
        if (playerInstanceList[playerIndex].GetComponent<PlayerStructure>().data.playerInputIndex != InputIndex.CPU)
        {
            RaceGUI playerRaceGui = GetRaceGUIFromPlayerInstance(playerInstanceList[playerIndex]);
            playerRaceGui.SetCanShowPauseMenu(true);
        }
    }

    public void HidePauseMenuForAllPlayers()
    {
        for (int i = 0; i < playerInstanceList.Count; i++)
        {
            if (playerInstanceList[i].GetComponent<PlayerStructure>().data.playerInputIndex != InputIndex.CPU)
            {
                RaceGUI playerRaceGui = GetRaceGUIFromPlayerInstance(playerInstanceList[i]);
                playerRaceGui.SetCanShowPauseMenu(false);
            }
        }
    }
    public void ShowResultForPlayer(int playerIndex)
    {
        if (playerInstanceList[playerIndex].GetComponent<PlayerStructure>().data.playerInputIndex != InputIndex.CPU)
        {
            GameObject winnerPlayer = playerInstanceList[playerIndex];

            RaceGUI playerRaceGui = GetRaceGUIFromPlayerInstance(winnerPlayer);
            playerRaceGui.SetCanShowResults(true);
        }
    }

    public void ShowCountdownForAllPlayers()
    {
        for (int i = 0; i < playerInstanceList.Count; i++)
        {
            if (playerInstanceList[i].GetComponent<PlayerStructure>().data.playerInputIndex != InputIndex.CPU)
            {
                RaceGUI playerRaceGui = GetRaceGUIFromPlayerInstance(playerInstanceList[i]);
                playerRaceGui.SetCanShowCountdown(true);
            }
        }
    }

    public void HideCountdownForAllPlayers()
    {
        for (int i = 0; i < playerInstanceList.Count; i++)
        {
            if (playerInstanceList[i].GetComponent<PlayerStructure>().data.playerInputIndex != InputIndex.CPU)
            {
                RaceGUI playerRaceGui = GetRaceGUIFromPlayerInstance(playerInstanceList[i]);
                playerRaceGui.SetCanShowCountdown(false);
            }
        }
    }

    public int GetPlayersAmount()
    {
        return playerInstanceList.Count;
    }

    public GameObject GetPlayerInstanceFromPlayerRaceData(PlayerRaceData playerRaceData)
    {
        string playerNae = playerRaceData.playerData.name;

        GameObject playerInstance = GetPlayerInstanceFromID(playerNae);

        return playerInstance;
    }
    
    public RaceGUI GetRaceGUIFromPlayerInstance(GameObject playerInstance)
    {
        if (playerInstance.GetComponent<PlayerStructure>().data.playerInputIndex == InputIndex.CPU) return null;
            PlayerStructure playerStructure = playerInstance.GetComponent<PlayerStructure>();
            GameObject playerRaceGUI = playerStructure.GetCanvasInstance();
            return playerRaceGUI.GetComponent<RaceGUI>();
    }

    public List<GameObject> GetAllPlayerInstances()
    {
        return playerInstanceList;
    }

}
