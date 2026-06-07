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

    public NamesLoader namesLoader;

    [Header("Default parameters")]
    public RaceMode mode;
    public List<PlayerData> playerDataList;
    public int maxLaps;
    public  List<GameObject> avaiableVeichleList;

    [Header("Audio parameters")]
    public AudioSource sountrackAudioSource;

    [Header("Checkpoint Generation")]
    public TextAsset checkpointSourceCSV;
    public Vector3 csvWorldOffset = Vector3.zero;
    public GameObject checkpointSourceCurve;
    public Transform finishLine;
    [Range(1, 3)] public int checkpointDensityMultiplier = 1;
    public float checkpointWidth = 20f;
    public float checkpointHeight = 5f;
    [Range(0.1f, 45f)] public float cornerCurvatureThreshold = 0.5f;
    [Range(3f, 400f)] public float maxAngularStep = 10f;
    [Range(10f, 400f)] public float straightCheckpointSpacing = 30f;
    [Range(0f, 30f)] public float cornerMergeThreshold = 8f;

    private RacePhase currentRacePhase;
    private List<GameObject> playerInstanceList;
    private List<int> sectorList;
    private RaceData raceData;
    private int humanPlayersAmount = 0;
    


    private bool isPaused = false;
    private bool isReady = false;

    private int pausePlayerIndex = -1;

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

    public List<GameObject> GetPlayerInstanceList()
    {
        return playerInstanceList;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {       

        for (int i = 0; i < playerDataList.Count; i++)
        {
            playerDataList[i].SetCPUIndex(i);
        }
        

        // Generating CPU Players if needed
        if (generateCPUPlayers)
        {

            List<String> availableNames = new List<string>();

            if (namesLoader != null)
            {
                availableNames = namesLoader.namesList;
            }

            for (int i = 0; i < cpuPlayersAmount; i++)
            {
                // Real CPU are after the player
                if(i >= playerDataList.Count)
                {
                    string displayName = "CPU" + (i);

                    if (availableNames.Count > 0)
                    {
                        int nameIndex = UnityEngine.Random.Range(0, availableNames.Count);
                        string name = availableNames[nameIndex];
                        availableNames.RemoveAt(nameIndex);
                        displayName = name;
                        
                    }
                    else
                    {
                        Debug.LogWarning($"[RaceManager] : No more names available for CPU players. Assigning default name for CPU player {i}");
                    }

                    int veichleIndex = UnityEngine.Random.Range(0, avaiableVeichleList.Count);

                    PlayerData cpuPlayerData = new PlayerData("CPU" + (i), avaiableVeichleList[veichleIndex], InputIndex.CPU, displayName);
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
                    playerRaceDataList.Add(new PlayerRaceData(playerDataList[i], 0, "", 0, 0, 1, true));

                }

                raceData = new RaceData(playerRaceDataList);
                playerInstanceList = new List<GameObject>();

                switch (mode)
                {
                    case RaceMode.Test:


                        addPlayerInstances();
                        break;
                    case RaceMode.TimeTrial:

                        GameObject newTimeTrialPlayer = InstantiatePlayer(playerDataList[0], spawnPointList[0]);

                        playerInstanceList.Add(newTimeTrialPlayer);

                        break;
                    case RaceMode.RaceSingleplayer:

                        addPlayerInstances();


                        break;
                    case RaceMode.RaceMultiplayer:

                        addPlayerInstances();

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
            gameObject.GetComponent<RaceDifficultyManager>().difficulty = currentRaceSettings.GetSelectedDifficulty();
        }

    }

    public GameObject InstantiatePlayer(PlayerData playerData, Transform spawnPoint) {

        Debug.Log($"[RaceManager] : Instantiating player {playerData.nameId} at position {spawnPoint.position}");

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

        Camera backgroundCamera = null;

        // --- BACKGROUND CAMERA (clear everything) ---
        if (backgroundCamera == null)
        {
            GameObject bg = new GameObject("BackgroundCamera");
            backgroundCamera = bg.AddComponent<Camera>();
        }

        backgroundCamera.depth = -100;
        backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
        backgroundCamera.backgroundColor = Color.black;
        backgroundCamera.rect = new Rect(0f, 0f, 1f, 1f);
        backgroundCamera.cullingMask = 0; // do not render anything

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

        Debug.Log("the winner is : " + raceData.playerRaceDataList[0].playerData.nameId + " (player index : " + (int)raceData.playerRaceDataList[0].playerData.playerInputIndex + " )");

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
            string playerId = playerInstanceList[i].GetComponent<PlayerStructure>().data.nameId;
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
            GameObject playerInstance = GetPlayerInstanceFromID(playerRaceData.playerData.nameId);

            // calculate new checkpoint distance
            Vector3 newDistanceVector = playerInstance.transform.position - checkPointList[playerRaceData.nextCheckpointIndex].transform.position;
            float distance = newDistanceVector.magnitude;

            playerRaceData.currentCheckpointDistance = distance;
        }
    }

    public void OnCheckpoint(string playerID, GameObject checkPointGameObject)
    {
        int playerDataToUpdateIndex = raceData.playerRaceDataList.FindIndex(p => p.playerData.nameId == playerID);
        PlayerRaceData currentPlayerRaceData = raceData.playerRaceDataList[playerDataToUpdateIndex];

        if (checkPointGameObject.transform.parent?.gameObject == checkPointList[currentPlayerRaceData.nextCheckpointIndex])
        {
            currentPlayerRaceData.nextCheckpointIndex++;

            // check if player has completed the lap
            if (currentPlayerRaceData.nextCheckpointIndex > checkPointList.Count - 1)
            {
                currentPlayerRaceData.nextCheckpointIndex = 0;
                currentPlayerRaceData.currentLap++;

                raceData.SetLapTimeForPlayer(playerDataToUpdateIndex);

                if (currentPlayerRaceData.currentLap > maxLaps && currentPlayerRaceData.inRace)
                {
                    managePlayerRaceEnd(currentPlayerRaceData);
                }
            }


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

            PlayerRaceData CPURaceData = raceData.GetPlayerRaceDataByID(playerInstance.GetComponent<PlayerStructure>().data.nameId);

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
        isPaused = true;
        ShowPauseMenuForInputPlayer(playerIndex);
        Time.timeScale = 0f;
        pausePlayerIndex = playerIndex;
    }

    public void OnPauseExit()
    {
        isPaused = false;
        Time.timeScale = 1f;
        HidePauseMenuForAllPlayers();
        pausePlayerIndex = -1;
    }

    public void OnReadyButtonPress(int playerIndex)
    {
        Debug.Log("[RaceManager] : Player" + (playerIndex + 1) + " pressed READY button");

        if (currentRacePhase == RacePhase.Presentation)
        {
            Debug.Log("[RaceManager] : Skip Presentation");
            TriggerRaceEvent(RacePhaseEvent.PresentationEnd);
        }
    }

    public void OnStartButtonPress(int playerIndex)
    {
        Debug.Log("[RaceManager] : Player" + (playerIndex + 1) + " pressed START button");
        if (currentRacePhase == RacePhase.Race)
        {
            Debug.Log("[RaceManager] : Pause button pressed");
            

            if (!isPaused) {
                if(pausePlayerIndex == -1)
                {
                    OnPauseEnter(playerIndex);
                    Debug.Log("[RaceManager] : Game paused");
                }
            }
            else
            {
                if(pausePlayerIndex != -1 && pausePlayerIndex == playerIndex)
                {
                    OnPauseExit();
                    Debug.Log("[RaceManager] : Game resumed");
                }
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

        Debug.Log("Showing results for player " + playerInstance.GetComponent<PlayerStructure>().data.nameId);

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
        string playerNae = playerRaceData.playerData.nameId;

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

#if UNITY_EDITOR
    // =====================================================
    // CHECKPOINT GENERATION FROM CURVE
    // Right-click RaceManager in Inspector → Generate Checkpoints from Curve
    // Requires: checkpointSourceCurve (FBX bezier exported without bevel from Blender)
    //           checkPointListParent (existing parent GO for checkpoint hierarchy)
    // =====================================================

    [ContextMenu("Generate Checkpoints from Curve")]
    public void GenerateCheckpointsFromCurve()
    {
        if (checkpointSourceCurve == null && checkpointSourceCSV == null)
        {
            Debug.LogError("[RaceManager] Assign either checkpointSourceCSV or checkpointSourceCurve.");
            return;
        }
        if (checkPointListParent == null)
        {
            Debug.LogError("[RaceManager] checkPointListParent not set.");
            return;
        }

        List<Vector3> rawPoints = ExtractCurvePoints(checkpointSourceCurve);
        if (rawPoints == null || rawPoints.Count < 2)
        {
            Debug.LogError("[RaceManager] Could not extract curve points.");
            return;
        }

        const float resampleStep = 0.5f;
        List<Vector3> path = ResamplePolyline(rawPoints, resampleStep);
        if (path.Count < 3)
        {
            Debug.LogError("[RaceManager] Resampled path too short.");
            return;
        }

        float[] curvatures = ComputeSmoothedCurvatures(path, resampleStep);

        // ── Pass 1: place all checkpoints, all typed Item ─────────────────
        // Only placement triggers (angular accumulator + distance fallback).
        // No type-specific forced indices — type assignment happens in pass 2.
        float straightInterval = straightCheckpointSpacing / checkpointDensityMultiplier;
        float angularThreshold = maxAngularStep / checkpointDensityMultiplier;
        const float minSpacing = 3f;
        float distSinceLast = straightInterval;
        float angularAcc    = 0f;
        Vector3 lastPos     = Vector3.positiveInfinity;

        var placed = new List<(int pathIndex, Vector3 pos, Vector3 fwd)>();

        for (int i = 0; i < path.Count; i++)
        {
            if (i > 0 && i < path.Count - 1)
            {
                Vector3 d1 = (path[i]     - path[i - 1]).normalized;
                Vector3 d2 = (path[i + 1] - path[i]).normalized;
                angularAcc += Vector3.Angle(d1, d2);
            }
            distSinceLast += resampleStep;

            if (angularAcc < angularThreshold && distSinceLast < straightInterval) continue;
            if ((path[i] - lastPos).sqrMagnitude < minSpacing * minSpacing) continue;

            int prev = Mathf.Max(i - 1, 0);
            int next = Mathf.Min(i + 1, path.Count - 1);
            Vector3 fwd = (path[next] - path[prev]).normalized;
            if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;

            placed.Add((i, path[i], fwd));
            lastPos       = path[i];
            distSinceLast = 0f;
            angularAcc    = 0f;
        }

        // ── Pass 2: assign corner types ───────────────────────────────────
        // A checkpoint is "in a corner" when the smoothed curvature at its path
        // index meets the threshold. Consecutive in-corner checkpoints form a run:
        //   run length 1  → CornerStart (directed entry with no apex/exit needed)
        //   run length 2  → CornerStart + CornerEnd
        //   run length 3+ → CornerStart, CornerMid…, CornerEnd
        var types = new CheckpointTypeEnum[placed.Count];
        for (int i = 0; i < placed.Count; i++)
            types[i] = CheckpointTypeEnum.Item;

        // For each placed checkpoint compute the peak curvature in the window that
        // spans from the midpoint to the previous checkpoint to the midpoint to the next.
        // This ensures a corner peak is attributed to the nearest checkpoint even when
        // the checkpoint landed tens of meters away from the geometric peak.
        float[] windowPeakCurv = new float[placed.Count];
        for (int i = 0; i < placed.Count; i++)
        {
            int lo = i == 0
                ? 0
                : (placed[i - 1].pathIndex + placed[i].pathIndex) / 2;
            int hi = i == placed.Count - 1
                ? curvatures.Length - 1
                : (placed[i].pathIndex + placed[i + 1].pathIndex) / 2;

            float peak = 0f;
            for (int k = lo; k <= hi; k++)
                if (curvatures[k] > peak) peak = curvatures[k];
            windowPeakCurv[i] = peak;
        }

        // Diagnostic log
        float maxPathCurvFound = 0f;
        foreach (float c in curvatures) if (c > maxPathCurvFound) maxPathCurvFound = c;
        int aboveThreshold = 0;
        var curvLog = new System.Text.StringBuilder();
        curvLog.AppendLine($"[RaceManager] Pass 2 — {placed.Count} checkpoints, threshold={cornerCurvatureThreshold:F2}°/m, maxPathCurv={maxPathCurvFound:F2}°/m");
        for (int i = 0; i < placed.Count; i++)
        {
            bool isCorner = windowPeakCurv[i] >= cornerCurvatureThreshold;
            if (isCorner) aboveThreshold++;
            curvLog.AppendLine($"  cp{i:D3} pathIdx={placed[i].pathIndex} windowPeak={windowPeakCurv[i]:F2}°/m {(isCorner ? ">>> CORNER" : "")}");
        }
        curvLog.AppendLine($"  Above threshold: {aboveThreshold}/{placed.Count}");
        Debug.Log(curvLog.ToString());

        int p = 0;
        while (p < placed.Count)
        {
            if (windowPeakCurv[p] < cornerCurvatureThreshold) { p++; continue; }

            int runStart = p;
            while (p < placed.Count && windowPeakCurv[p] >= cornerCurvatureThreshold) p++;
            int runEnd = p - 1;
            int runLen = runEnd - runStart + 1;

            if (runLen == 1)
            {
                types[runStart] = CheckpointTypeEnum.CornerStart;
            }
            else
            {
                types[runStart] = CheckpointTypeEnum.CornerStart;
                types[runEnd]   = CheckpointTypeEnum.CornerEnd;
                for (int k = runStart + 1; k < runEnd; k++)
                    types[k] = CheckpointTypeEnum.CornerMid;

                // Compact corner: if run spans less than cornerMergeThreshold,
                // override CornerMid forwards to point from entry toward exit.
                if (cornerMergeThreshold > 0f)
                {
                    float runDist = Vector3.Distance(placed[runStart].pos, placed[runEnd].pos);
                    if (runDist < cornerMergeThreshold)
                    {
                        Vector3 throughDir = (placed[runEnd].pos - placed[runStart].pos).normalized;
                        if (throughDir.sqrMagnitude > 0.001f)
                            for (int k = runStart + 1; k < runEnd; k++)
                                placed[k] = (placed[k].pathIndex, placed[k].pos, throughDir);
                    }
                }
            }
        }

        // ── Commit ────────────────────────────────────────────────────────
        for (int i = checkPointListParent.transform.childCount - 1; i >= 0; i--)
            UnityEditor.Undo.DestroyObjectImmediate(checkPointListParent.transform.GetChild(i).gameObject);

        int cpIndex = 0;
        for (int i = 0; i < placed.Count; i++)
            CreateCheckpointGO(placed[i].pos, placed[i].fwd, types[i], cpIndex++);

        if (finishLine != null)
            CreateCheckpointGO(finishLine.position, finishLine.forward, CheckpointTypeEnum.Item, cpIndex++);
        else
            Debug.LogWarning("[RaceManager] finishLine not set — lap detection will not work correctly.");

        checkPointList.Clear();
        for (int i = 0; i < checkPointListParent.transform.childCount; i++)
            checkPointList.Add(checkPointListParent.transform.GetChild(i).gameObject);

        UnityEditor.EditorUtility.SetDirty(gameObject);

        int nStarts = 0, nMids = 0, nEnds = 0;
        for (int i = 0; i < types.Length; i++)
        {
            if (types[i] == CheckpointTypeEnum.CornerStart) nStarts++;
            else if (types[i] == CheckpointTypeEnum.CornerMid) nMids++;
            else if (types[i] == CheckpointTypeEnum.CornerEnd) nEnds++;
        }
        Debug.Log($"[RaceManager] Generated {cpIndex} checkpoints — {nStarts} corners (Start/Mid/End: {nStarts}/{nMids}/{nEnds}), finish at index {cpIndex - 1}.");
    }

    private List<Vector3> ExtractCurvePoints(GameObject curveGO)
    {
        // --- 0. CSV exported from Blender via Python script (most reliable) ---
        // curveGO may be null when only the CSV is used
        if (checkpointSourceCSV != null)
        {
            var pts = new List<Vector3>();
            foreach (string raw in checkpointSourceCSV.text.Split('\n'))
            {
                string line = raw.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                string[] parts = line.Split(',');
                if (parts.Length == 3
                    && float.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float x)
                    && float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float y)
                    && float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float z))
                {
                    pts.Add(new Vector3(x, y, z) - csvWorldOffset);
                }
            }
            if (pts.Count >= 2)
            {
                Debug.Log($"[RaceManager] CSV: {pts.Count} points loaded.");
                return pts;
            }
            Debug.LogWarning("[RaceManager] CSV assigned but no valid points found.");
        }

        // --- 1. Try to read from a MeshFilter (FBX mesh or converted curve) ---
        if (curveGO == null) return null;
        MeshFilter mf = curveGO.GetComponentInChildren<MeshFilter>();
        if (mf != null && mf.sharedMesh != null && mf.sharedMesh.vertexCount > 0)
        {
            Mesh mesh = mf.sharedMesh;
            Matrix4x4 l2w = mf.transform.localToWorldMatrix;

            if (mesh.triangles.Length > 0)
            {
                // Tube/ribbon mesh (curve exported with bevel from Blender).
                // Extract spine by grouping vertices into cross-section rings and averaging each ring.
                Debug.LogWarning("[RaceManager] Curve mesh has faces — using ring-centroid spine extraction. " +
                    "For best results, export from Blender with Object > Convert > Mesh (no bevel).");
                return ExtractSpineFromTubeMesh(mesh, l2w);
            }

            // Edge-only mesh (bezier converted to mesh without bevel) — vertices are path points.
            const float epsilon = 0.01f;
            List<Vector3> unique = new List<Vector3>();
            foreach (Vector3 v in mesh.vertices)
            {
                Vector3 w = l2w.MultiplyPoint3x4(v);
                bool isDup = false;
                for (int i = 0; i < unique.Count; i++)
                    if ((w - unique[i]).sqrMagnitude < epsilon * epsilon) { isDup = true; break; }
                if (!isDup) unique.Add(w);
            }
            if (unique.Count >= 2)
                return OrderByNearestNeighbour(unique);
        }

        // --- 2. Fallback: child transforms as waypoints (empties placed along spline in Blender) ---
        if (curveGO.transform.childCount >= 2)
        {
            Debug.Log("[RaceManager] No mesh found — reading child transforms as waypoints.");
            List<Vector3> childPositions = new List<Vector3>();
            for (int i = 0; i < curveGO.transform.childCount; i++)
                childPositions.Add(curveGO.transform.GetChild(i).position);
            return childPositions;
        }

        // --- Nothing worked ---
        Debug.LogError(
            $"[RaceManager] No curve data found on '{curveGO.name}'.\n" +
            "In Blender, do ONE of the following before exporting FBX:\n" +
            "  A) Select the bezier curve → Object > Convert > Mesh  (recommended)\n" +
            "  B) Set Bevel Depth > 0 in Object Data Properties and enable Apply Modifiers in FBX export\n" +
            "  C) Place Empty objects as children of the curve object along the path");
        return null;
    }

    // Extracts the centerline spine from a tube mesh (bezier curve with bevel from Blender).
    // Step 1: classify edges as circumferential (short) or longitudinal (long).
    // Step 2: find cross-section rings via connected components of short edges.
    // Step 3: build ring adjacency graph via long edges — topology-based, no FBX vertex order assumptions.
    // Step 4: chain-traverse the ring graph to get spine order.
    private List<Vector3> ExtractSpineFromTubeMesh(Mesh mesh, Matrix4x4 l2w)
    {
        Vector3[] verts = mesh.vertices;
        int n = verts.Length;

        Vector3[] world = new Vector3[n];
        for (int i = 0; i < n; i++)
            world[i] = l2w.MultiplyPoint3x4(verts[i]);

        int[] tris = mesh.triangles;

        // Collect unique edges with (min,max) tuple key
        var edgeLengths = new Dictionary<(int, int), float>();
        for (int t = 0; t < tris.Length; t += 3)
        {
            for (int e = 0; e < 3; e++)
            {
                int a = tris[t + e];
                int b = tris[t + (e + 1) % 3];
                var key = a < b ? (a, b) : (b, a);
                if (!edgeLengths.ContainsKey(key))
                    edgeLengths[key] = (world[a] - world[b]).magnitude;
            }
        }

        if (edgeLengths.Count == 0) return null;

        var lengths = new List<float>(edgeLengths.Values);
        lengths.Sort();
        // Find the largest gap in the sorted edge-length distribution — separates the circumferential
        // (short, ~bevel perimeter) from longitudinal (long, ring-to-ring) populations.
        // Midpoint (min+max)*0.5 breaks when tight curve sections have ring spacing < threshold.
        float shortThreshold = lengths[lengths.Count - 1]; // fallback: treat all as short
        float maxGap = 0f;
        for (int g = 1; g < lengths.Count; g++)
        {
            float gap = lengths[g] - lengths[g - 1];
            if (gap > maxGap) { maxGap = gap; shortThreshold = (lengths[g - 1] + lengths[g]) * 0.5f; }
        }

        // Build adjacency for circumferential (short) edges only
        var shortAdj = new Dictionary<int, List<int>>();
        foreach (var kv in edgeLengths)
        {
            if (kv.Value >= shortThreshold) continue;
            int a = kv.Key.Item1, b = kv.Key.Item2;
            if (!shortAdj.ContainsKey(a)) shortAdj[a] = new List<int>();
            if (!shortAdj.ContainsKey(b)) shortAdj[b] = new List<int>();
            shortAdj[a].Add(b);
            shortAdj[b].Add(a);
        }

        // Find rings via connected components of short edges
        var ringCentroids  = new List<Vector3>();
        var ringVertexLists = new List<List<int>>();
        var visited = new HashSet<int>();

        for (int i = 0; i < n; i++)
        {
            if (visited.Contains(i)) continue;

            var ring = new List<int>();
            var queue = new Queue<int>();
            queue.Enqueue(i);
            visited.Add(i);

            while (queue.Count > 0)
            {
                int v = queue.Dequeue();
                ring.Add(v);
                if (!shortAdj.ContainsKey(v)) continue;
                foreach (int nb in shortAdj[v])
                    if (!visited.Contains(nb)) { visited.Add(nb); queue.Enqueue(nb); }
            }

            if (ring.Count < 2) continue;

            Vector3 centroid = Vector3.zero;
            foreach (int idx in ring) centroid += world[idx];
            ringCentroids.Add(centroid / ring.Count);
            ringVertexLists.Add(ring);
        }

        int numRings = ringCentroids.Count;
        if (numRings < 2) return null;

        // Map each vertex to its ring index
        int[] vertexToRing = new int[n];
        for (int r = 0; r < numRings; r++)
            foreach (int v in ringVertexLists[r])
                vertexToRing[v] = r;

        // Build ring adjacency graph from long (longitudinal) edges
        var ringAdj = new List<HashSet<int>>(numRings);
        for (int r = 0; r < numRings; r++) ringAdj.Add(new HashSet<int>());

        foreach (var kv in edgeLengths)
        {
            if (kv.Value < shortThreshold) continue;
            int ra = vertexToRing[kv.Key.Item1];
            int rb = vertexToRing[kv.Key.Item2];
            if (ra != rb) { ringAdj[ra].Add(rb); ringAdj[rb].Add(ra); }
        }

        // Start from a degree-1 ring (endpoint of open curve); fall back to ring 0 for closed loops
        int startRing = 0;
        for (int r = 0; r < numRings; r++)
            if (ringAdj[r].Count == 1) { startRing = r; break; }

        // Chain traversal: each ring connects to at most 2 neighbors (prev / next along spine)
        var orderedResult = new List<Vector3>(numRings);
        var visitedRings  = new HashSet<int>();
        int cur = startRing;
        while (cur >= 0)
        {
            orderedResult.Add(ringCentroids[cur]);
            visitedRings.Add(cur);
            int next = -1;
            foreach (int adj in ringAdj[cur])
                if (!visitedRings.Contains(adj)) { next = adj; break; }
            cur = next;
        }

        Debug.Log($"[RaceManager] Tube spine: {orderedResult.Count}/{numRings} rings ordered via topology.");
        return orderedResult;
    }

    private List<Vector3> OrderByNearestNeighbour(List<Vector3> pts)
    {
        List<Vector3> ordered = new List<Vector3>(pts.Count);
        bool[] visited = new bool[pts.Count];
        int cur = 0;
        visited[0] = true;
        ordered.Add(pts[0]);

        for (int step = 1; step < pts.Count; step++)
        {
            float best = float.MaxValue;
            int next = -1;
            for (int j = 0; j < pts.Count; j++)
            {
                if (visited[j]) continue;
                float d = (pts[j] - pts[cur]).sqrMagnitude;
                if (d < best) { best = d; next = j; }
            }
            if (next < 0) break;
            visited[next] = true;
            ordered.Add(pts[next]);
            cur = next;
        }

        return ordered;
    }

    private List<Vector3> ResamplePolyline(List<Vector3> pts, float step)
    {
        List<Vector3> result = new List<Vector3> { pts[0] };
        float acc = 0f;

        for (int i = 1; i < pts.Count; i++)
        {
            float segLen = (pts[i] - pts[i - 1]).magnitude;
            if (segLen < 0.0001f) continue;
            acc += segLen;

            while (acc >= step)
            {
                acc -= step;
                float t = 1f - acc / segLen;
                result.Add(Vector3.Lerp(pts[i - 1], pts[i], t));
            }
        }

        return result;
    }

    private float[] ComputeSmoothedCurvatures(List<Vector3> pts, float step)
    {
        int n = pts.Count;
        float[] raw = new float[n];

        for (int i = 1; i < n - 1; i++)
        {
            Vector3 d1 = (pts[i]     - pts[i - 1]).normalized;
            Vector3 d2 = (pts[i + 1] - pts[i]).normalized;
            raw[i] = Vector3.Angle(d1, d2) / step; // degrees per meter
        }

        // 11-point moving average
        float[] smoothed = new float[n];
        const int half = 5;
        for (int i = 0; i < n; i++)
        {
            float sum = 0f;
            int count = 0;
            for (int j = Mathf.Max(0, i - half); j <= Mathf.Min(n - 1, i + half); j++)
            {
                sum += raw[j];
                count++;
            }
            smoothed[i] = sum / count;
        }

        return smoothed;
    }

    private List<(int start, int mid, int end)> FindCornerRegions(float[] curvatures, float threshold)
    {
        var corners = new List<(int, int, int)>();
        int n = curvatures.Length;
        bool inCorner = false;
        int cStart = 0, cMid = 0;
        float peak = 0f;

        for (int i = 0; i < n; i++)
        {
            if (!inCorner && curvatures[i] >= threshold)
            {
                inCorner = true;
                cStart = i;
                cMid = i;
                peak = curvatures[i];
            }
            else if (inCorner)
            {
                if (curvatures[i] > peak) { peak = curvatures[i]; cMid = i; }

                if (curvatures[i] < threshold || i == n - 1)
                {
                    inCorner = false;
                    int cEnd = (i - 1 > cMid) ? i - 1 : cMid;
                    corners.Add((cStart, cMid, cEnd));
                }
            }
        }

        if (inCorner)
            corners.Add((cStart, cMid, n - 1));

        return corners;
    }

    private void CreateCheckpointGO(Vector3 position, Vector3 forward, CheckpointTypeEnum type, int index)
    {
        int layer = LayerMask.NameToLayer("IgnoreHoverRaycast");
        if (layer == -1)
        {
            Debug.LogWarning("[RaceManager] Layer 'IgnoreHoverRaycast' not found. Add it in Project Settings > Tags and Layers.");
            layer = 0;
        }

        GameObject cpGO = new GameObject($"Checkpoint_{index:D3}");
        UnityEditor.Undo.RegisterCreatedObjectUndo(cpGO, "Generate Checkpoint");
        cpGO.transform.SetParent(checkPointListParent.transform, true);
        cpGO.transform.position = position;
        cpGO.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        cpGO.layer = layer;

        CheckpointType ct = cpGO.AddComponent<CheckpointType>();
        ct.checkpointType = type;

        GameObject triggerGO = new GameObject("Trigger");
        UnityEditor.Undo.RegisterCreatedObjectUndo(triggerGO, "Generate Checkpoint Trigger");
        triggerGO.transform.SetParent(cpGO.transform, false);
        triggerGO.layer = layer;

        try { triggerGO.tag = "Checkpoint"; }
        catch { Debug.LogWarning("[RaceManager] Tag 'Checkpoint' not defined. Add it in Project Settings > Tags and Layers."); }

        BoxCollider col = triggerGO.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(checkpointWidth, checkpointHeight, 1f);
    }
#endif

}
