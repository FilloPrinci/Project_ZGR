using NUnit.Framework;
using System;
using System.Collections;
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
    public Collider trackMainCollider;
    public GameObject checkPointListParent;
    public List<GameObject> checkPointList;

    public GameObject raceGrid;
    public List<Transform> spawnPointList;
    
    public GameObject presentationManager;
    public PlayersCollisionDetection playersCollisionDetection;

    [Header("Default parameters")]
    public RaceMode mode;
    public List<PlayerData> playerDataList;
    public int maxLaps;
    public  List<GameObject> avaiableVeichleList;

    [Header("Audio parameters")]
    public AudioSource sountrackAudioSource;

    private int currentLap;
    private RacePhase currentRacePhase;
    private RacePhaseEvent lastRacePhaseEvent;
    private List<GameObject> playerInstanceList;
    private List<int> sectorList;
    private RaceData raceData;
    private int humanPlayersAmount = 0;
    


    private bool isPaused = false;
    private bool isReady = false;

    public RaceData GetRaceData()
    {
        return raceData;
    }

    public RacePhase GetCurrentRacePhase()
    {
        return currentRacePhase;   
    }

    public int GetHumanPlayersAmount()
    {
        return humanPlayersAmount;
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    public bool IsReady()
    {
        return isReady;
    }

    private void OnValidate()
    {
        if (checkPointListParent != null)
        {
            checkPointList.Clear();
            int childCount = checkPointListParent.transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                checkPointList.Add(checkPointListParent.transform.GetChild(i).gameObject);
            }
        }

        if (raceGrid != null) {
            spawnPointList.Clear();
            int childCount = raceGrid.transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                spawnPointList.Add(raceGrid.transform.GetChild(i).gameObject.transform);
            }
        }
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
                cpuPlayersAmount = spawnPointList.Count;
                Debug.Log($"[RaceManager] : CPU Players amount set to {cpuPlayersAmount} based on spawnPointList");
            }
            else
            {
                cpuPlayersAmount += playerDataList.Count;
            }
        }
        LoadRaceSettings();

        humanPlayersAmount = playerDataList.Count;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentLap = 0;

        

        for (int i = 0; i < playerDataList.Count; i++)
        {
            playerDataList[i].SetCPUIndex(i);
        }
        

        // Generating CPU Players if needed
        if (generateCPUPlayers)
        {
            for (int i = 0; i < cpuPlayersAmount; i++)
            {
                // Real CPU are after the player
                if(i >= playerDataList.Count)
                {
                    int veichleIndex = UnityEngine.Random.Range(0, avaiableVeichleList.Count);

                    PlayerData cpuPlayerData = new PlayerData("CPU" + (i), avaiableVeichleList[veichleIndex], InputIndex.CPU);
                    cpuPlayerData.SetCPUIndex(i);
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
                    playerRaceDataList.Add(new PlayerRaceData(playerDataList[i], 0, "", 0, 0, 0, 1, true));

                }

                raceData = new RaceData(playerRaceDataList);
                playerInstanceList = new List<GameObject>();

                switch (mode)
                {
                    case RaceMode.Test:


                        addPlayerInstances();
                        lastRacePhaseEvent = RacePhaseEvent.Start;
                        break;
                    case RaceMode.TimeTrial:

                        GameObject newTimeTrialPlayer = InstantiatePlayer(playerDataList[0], spawnPointList[0]);

                        playerInstanceList.Add(newTimeTrialPlayer);

                        lastRacePhaseEvent = RacePhaseEvent.Start;
                        break;
                    case RaceMode.RaceSingleplayer:

                        addPlayerInstances();

                        lastRacePhaseEvent = RacePhaseEvent.Start;

                        break;
                    case RaceMode.RaceMultiplayer:

                        addPlayerInstances();


                        lastRacePhaseEvent = RacePhaseEvent.Start;
                        break;
                }
            }
            TriggerRaceEvent(RacePhaseEvent.Start);
        }

        isReady = true;

    }
    
    private void addPlayerInstances(bool keepOrder = false)
    {
        if (keepOrder)
        {
            for (int i = 0; i < playerDataList.Count; i++)
            {
                GameObject newPlayer = InstantiatePlayer(playerDataList[i], spawnPointList[i]);

                playerInstanceList.Add(newPlayer);
            }
        }
        else
        {
            List<PlayerData> cpuData = new List<PlayerData>();
            List<PlayerData> playerData = new List<PlayerData>();

            foreach (PlayerData data in playerDataList) { 
                if(data.playerInputIndex == InputIndex.CPU)
                {
                    cpuData.Add(data);
                }
                else
                {
                    playerData.Add(data);
                }
            }
            
            for (int i = 0; i < playerDataList.Count; i++)
            {

                GameObject newPlayer;
                // first spawn CPU
                if (i < cpuData.Count)
                {
                    cpuData[i].cpuIndex = i;
                    newPlayer = InstantiatePlayer(cpuData[i], spawnPointList[i]);
                }
                // then spawn Players
                else
                {
                    int playerIndex = i - cpuData.Count;
                    Debug.Log("assigning cpuIndex: " + playerIndex);
                    playerData[playerIndex].cpuIndex = i;
                    newPlayer = InstantiatePlayer(playerData[playerIndex], spawnPointList[i]);
                }

                playerInstanceList.Add(newPlayer);
            }
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

        playersCollisionDetection.InitializePlayersColliders(getPlayerControllerList());

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
                    Debug.Log("[RaceManager] : Activating multiplayer camera mode... humanPlayersAmount: " + humanPlayersAmount);

                    if (humanPlayersAmount == 2)
                    {
                        instanceStructure.ActivatePlayerCamera(CameraMode.MultiPlayer2);
                    }else if (humanPlayersAmount == 3)
                    {
                        instanceStructure.ActivatePlayerCamera(CameraMode.MultiPlayer3);
                    }else if(humanPlayersAmount == 4)
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
        AudioListener presentationCameraAudioListener = mainCamera.GetComponent<AudioListener>();
            if (presentationCameraAudioListener != null) {
            Destroy(presentationCameraAudioListener);
        }

        if(sountrackAudioSource != null)
        {
            sountrackAudioSource.Stop();
            sountrackAudioSource.Play();
        }
            

        HideCountdownForAllPlayers();
        raceData.StartRace();
        StartCoroutine(UpdateRankingCoroutine());
    }

    private void OnRaceEnd()
    {
        if (sountrackAudioSource != null)
        {
            sountrackAudioSource.Stop();
        }

        raceData.playerRaceDataList.Sort((a, b) => a.position.CompareTo(b.position));

        Debug.Log("the winner is : " + raceData.playerRaceDataList[0].playerData.name + " (player index : " + (int)raceData.playerRaceDataList[0].playerData.playerInputIndex + " )");

        foreach (PlayerRaceData playerRaceData in raceData.playerRaceDataList)
        {
            if (playerRaceData.playerData.playerInputIndex != InputIndex.CPU)
            {
                ShowResultForPlayer(playerRaceData);


                // finish race for all playuers
                foreach (GameObject playerInstance in playerInstanceList)
                {
                    if (playerInstance.GetComponent<PlayerStructure>().data.playerInputIndex != InputIndex.CPU)
                    {
                        PlayerStructure playerStructure = playerInstance.GetComponent<PlayerStructure>();
                        playerStructure.OnRaceEndPhase((int)playerRaceData.playerData.playerInputIndex);
                    }
                }
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

        
        raceData.UpdatePlayerRaceDataTimings();
    }

    IEnumerator UpdateRankingCoroutine()
    {
        var wait = new WaitForSeconds(1f/30f); // 30 Hz
        while (true)
        {
            RefreshPlayerRaceDataDistances();
            raceData.RefreshPositions();
            yield return wait;
        }
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
        PlayerRaceData currentPlayerRaceData = raceData.playerRaceDataList[playerDataToUpdateIndex];

        if (checkPointGameObject.transform.parent?.gameObject == checkPointList[currentPlayerRaceData.nextCheckpointIndex])
        {
            currentPlayerRaceData.currentSectorIndex++;

            // check if player has completed the lap
            if (currentPlayerRaceData.currentSectorIndex > checkPointList.Count - 1 )
            {
                // player has completed the lap

                currentPlayerRaceData.currentSectorIndex = 0;
                currentPlayerRaceData.currentLap++;

                raceData.SetLapTimeForPlayer(playerDataToUpdateIndex);

                if (currentPlayerRaceData.currentLap > maxLaps && currentPlayerRaceData.inRace)
                {
                    // player has finished the race
                    managePlayerRaceEnd(currentPlayerRaceData);


                }
            }
                
            currentPlayerRaceData.nextCheckpointIndex = currentPlayerRaceData.currentSectorIndex;


        }
        else
        {
            //Debug.Log($"[RaceManager] : Player {playerID} hit wrong checkpoint. Expected {currentPlayerRaceData.nextCheckpointIndex}, got {Array.IndexOf(checkPointList.ToArray(), checkPointGameObject.transform.parent?.gameObject)}");
        }

    }

    void managePlayerRaceEnd(PlayerRaceData currentPlayerRaceData)
    {
        // register player result
        raceData.AddFinalResultForPlayerRaceData(currentPlayerRaceData);

        // disable player controls
        currentPlayerRaceData.inRace = false;

        GameObject playerInstance = GetPlayerInstanceFromPlayerRaceData(currentPlayerRaceData);
        playerInstance.GetComponent<PlayerController>().EndRace();



        if (playerInstance.GetComponent<PlayerController>().playerData.playerInputIndex != InputIndex.CPU)
        {
            // real player ended his race
            playerInstance.GetComponent<PlayerController>().EndRace();

            if (mode == RaceMode.RaceSingleplayer)
            {
                // race is completed, register all positions and finish race for all the CPUs
                foreach (PlayerRaceData playerRaceData in raceData.playerRaceDataList)
                {
                    if (playerRaceData.inRace && playerRaceData != currentPlayerRaceData)
                    {
                        raceData.AddFinalResultForPlayerRaceData(playerRaceData);
                        playerRaceData.inRace = false;
                    }
                }
            }
            else if (mode == RaceMode.RaceMultiplayer)
            {
                // wait for other players to finish, register only this player position


                // check if this is the last player
            }

            RaceGUI playerRaceGUI = GetRaceGUIFromPlayerInstance(playerInstance);
            if (playerRaceGUI != null)
            {
                playerRaceGUI.Finish();
                playerRaceGUI.SetCanShowPositionResult(true);
            }
        }
        else
        {
            // CPU ended his race

            PlayerRaceData CPURaceData = raceData.GetPlayerRaceDataByID(playerInstance.GetComponent<PlayerStructure>().data.name);

            if (CPURaceData.inRace)
            {
                raceData.AddFinalResultForPlayerRaceData(CPURaceData);
                CPURaceData.inRace = false;
            }
        }

        // finish race
        if (isRaceEnded())
        {
            TriggerRaceEvent(RacePhaseEvent.RaceEnd);
        }
    }

    bool isRaceEnded()
    {

        // Race is over only when all the players ended the race
        bool isRaceEnded = true;
        foreach (PlayerRaceData playerRasceData in raceData.playerRaceDataList)
        {
            isRaceEnded = isRaceEnded && !playerRasceData.inRace;
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

                OnRaceEnd();
                

                           

                
                break;
        }
    }

    public void ExitRace()
    {
        GlobalInputManager.Instance.RemovePlayerInputInstances();
    }

    public void OnPauseEnter(int playerIndex)
    {
        ShowPauseMenuForInputPlayer(playerIndex);
        Time.timeScale = 0f;
        
    }

    public void OnPauseExit()
    {
        isPaused = false;
        Time.timeScale = 1f;
        HidePauseMenuForAllPlayers();
    }

    public void OnStartButtonPress(int playerIndex)
    {
        Debug.Log("[RaceManager] : Player" + (playerIndex + 1) + " pressed START button");

        if (currentRacePhase == RacePhase.Presentation)
        {
            Debug.Log("[RaceManager] : Skip Presentation");
            TriggerRaceEvent(RacePhaseEvent.PresentationEnd);
        }
        else if (currentRacePhase == RacePhase.Race)
        {
            Debug.Log("[RaceManager] : Pause button pressed");
            isPaused = !isPaused;

            if (isPaused) {
                OnPauseEnter(playerIndex);
                Debug.Log("[RaceManager] : Game paused");
            }
            else
            {
                OnPauseExit();
                Debug.Log("[RaceManager] : Game resumed");
            }
        }
        else if (currentRacePhase == RacePhase.Results)
        {
            Debug.Log("[RaceManager] : Exit from race");
        }
    }

    public void ShowPauseMenuForInputPlayer(int playerIndex)
    {
        GameObject playerInstance = GetPlayerInstanceFromPlayerInputIndex(playerIndex);

        PlayerStructure playerStructure = playerInstance.GetComponent<PlayerStructure>();

        if(playerStructure != null)
        {
            if (playerStructure.data.playerInputIndex != InputIndex.CPU)
            {
                Debug.Log("[RaceManager] : Showing pause menu for player " + playerStructure.name);
                RaceGUI playerRaceGui = GetRaceGUIFromPlayerInstance(playerInstance);
                if (playerRaceGui != null)
                {
                    playerRaceGui.SetCanShowPauseMenu(true);
                }
                else
                {
                    Debug.LogError("[RaceManager] : No RaceGUI found for player " + playerStructure.name);
                }
            }
        }
        else
        {
            Debug.LogError("[RaceManager] : No PlayerStructure found for player index " + playerIndex);
            return;
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
    public void ShowResultForPlayer(PlayerRaceData playerDaceData)
    {
        GameObject playerInstance =  GetPlayerInstanceFromPlayerRaceData(playerDaceData);

        Debug.Log("Showing results for player " + playerInstance.GetComponent<PlayerStructure>().data.name);

        if (playerInstance.GetComponent<PlayerStructure>().data.playerInputIndex != InputIndex.CPU)
        {

            RaceGUI playerRaceGui = GetRaceGUIFromPlayerInstance(playerInstance);
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

    public List<PlayerController> getPlayerControllerList()
    {
        List<PlayerController> playerControllerList = new List<PlayerController>();
        foreach (GameObject playerInstance in playerInstanceList)
        {
            PlayerController playerController = playerInstance.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerControllerList.Add(playerController);
            }
        }
        return playerControllerList;
    }

    public List<GameObject> GetAllPlayerInstances()
    {
        return playerInstanceList;
    }

    public GameObject GetPlayerInstanceFromPlayerInputIndex(int inputIndex) {
        foreach (GameObject playerInstance in playerInstanceList)
        {
            if ((int)playerInstance.GetComponent<PlayerStructure>().data.playerInputIndex == inputIndex)
            {
                return playerInstance;
            }
        }

        return null;
    }

}
