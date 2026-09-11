using System.Collections.Generic;
using UnityEngine;



public class RaceSettings : MonoBehaviour
{
    public static RaceSettings Instance { get; private set; }

    [Header("Players")]
    public int inputPlayersAmount = 1;
    public int totalPlayersAmount = 10;
    public List<PlayerData> cpuPlayerDataList;
    public List<GameObject> vehiclePrefabList;
    public int defaultVehicleIndex = 0;
    

    [Header("Race")]
    public int laps = 3;
    public int defaultRaceTrackIndex = 0;

    private SceneReferences sceneReferences;
    private List<PlayerData> inputPlayerDataList;
    private RaceMode selectedRaceMode = RaceMode.Test;
    private GlobalDifficulty selectedDifficulty = GlobalDifficulty.normal;
    private string selectedRaceTrack;
    private string selectedRaceTrackDisplayName;

    [Header("Trophy / Points")]
    // Points awarded for finishing in each position (index 0 = 1st place). Tune in Inspector.
    public List<int> pointsTable = new List<int> { 15, 12, 10, 8, 6, 4, 2, 1 };

    // CPU identity persistence: a CPU slot (cpuIndex) keeps the same name across races within
    // the same session/trophy. Cleared in ResetSettings() (end of session, back to main menu).
    private List<string> availableCPUNames; // null = not initialized yet
    private Dictionary<int, string> cpuNameByIndex = new Dictionary<int, string>();

    // Trophy points totals, keyed by PlayerData.nameId. Accumulates across races within a
    // session/trophy; cleared in ResetSettings().
    private Dictionary<string, int> totalPointsByPlayerId = new Dictionary<string, int>();


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
                    }
        else
        {
            Debug.LogWarning("Duplicate RaceSettings detected. Destroying extra instance.");
            Destroy(gameObject); // Assicura che ci sia solo un RaceSettings
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sceneReferences = SceneReferences.Instance;
        inputPlayerDataList = new List<PlayerData>();
        selectedRaceTrack = sceneReferences.trackSceneDataList[defaultRaceTrackIndex].sceneName;

        if (vehiclePrefabList.Count == 0)
        {
            Debug.LogError("[RaceSettings] ERROR: vehiclePrefabList is empty");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSelectedDifficulty(GlobalDifficulty newDifficulty)
    {
        selectedDifficulty = newDifficulty;
        Debug.Log("[RaceSettings] INFO: selected difficulty set to " + selectedDifficulty);
    }

    public GlobalDifficulty GetSelectedDifficulty()
    {
        return selectedDifficulty;
    }

    public RaceMode GetSelectedRaceMode()
    {
        return selectedRaceMode;
    }

    public string GetSelectedRaceTrack()
    {
        return selectedRaceTrack;
    }

    public string GetSelectedRaceTrackDisplayName()
    {
        return !string.IsNullOrEmpty(selectedRaceTrackDisplayName) ? selectedRaceTrackDisplayName : selectedRaceTrack;
    }

    public List<PlayerData> GetAllPlayerDataList()
    {
        List<PlayerData> completePlayerDataList = new List<PlayerData>();
        completePlayerDataList.AddRange(inputPlayerDataList);
        completePlayerDataList.AddRange(cpuPlayerDataList);

        return completePlayerDataList;
    }

    public void OnSinglePlayerSelect()
    {
        inputPlayersAmount = 1;
        PlayerData inputPlayerData = new PlayerData("Player1", vehiclePrefabList[defaultVehicleIndex], InputIndex.HID0);
        inputPlayerDataList.Add(inputPlayerData);
    }

    public void OnTimeTrialModeSelect()
    {
        selectedRaceMode = RaceMode.TimeTrial;
    }

    public void OnSingleplayerRaceModeSelect()
    {
        selectedRaceMode = RaceMode.RaceSingleplayer;
    }

    public void OnMultiplayerRaceModeSelect()
    {
        selectedRaceMode = RaceMode.RaceMultiplayer;
    }

    public void OnMultiplayerAmountSelect(int amount)
    {
        inputPlayersAmount = amount;

        inputPlayerDataList = new List<PlayerData>();

        for (int i = 0; i < inputPlayersAmount; i++) {
            string playerName = $"Player{i + 1}";
            GameObject playrVehicle = vehiclePrefabList[defaultVehicleIndex];
            InputIndex playerInputIndex = (InputIndex)i;
            PlayerData inputPlayerData = new PlayerData(playerName, playrVehicle, playerInputIndex);

            inputPlayerDataList.Add(inputPlayerData);
        }
    }

    public void OnVehicleSelect(int playerIndex, int vehicleIndex)
    {
        inputPlayerDataList[playerIndex].vehiclePrefab = vehiclePrefabList[vehicleIndex];
    }

    public void OnRaceTrackSelect(int trackIndex)
    {
        TrackSceneData trackData = sceneReferences.trackSceneDataList[trackIndex];
        selectedRaceTrack = trackData.sceneName;
        selectedRaceTrackDisplayName = trackData.displayName;
        Debug.Log("[RaceSettings] INFO: selected track set to " + selectedRaceTrack);
    }

    public void SetSelectedVehicleForPlayer(int playerIndex, GameObject vehiclePrefab)
    {
        inputPlayerDataList[playerIndex].vehiclePrefab = vehiclePrefab;
        Debug.Log("[RaceSettings] INFO: player " + inputPlayerDataList[playerIndex].nameId + " has selected vehicle " + vehiclePrefab.name);
    }

    public void ResetSettings()
    {
        inputPlayersAmount = 1;
        totalPlayersAmount = 10;
        //laps = 3;
        cpuPlayerDataList = new List<PlayerData>();
        inputPlayerDataList = new List<PlayerData>();
        selectedRaceMode = RaceMode.Test;

        // End of session/trophy: forget CPU identities and accumulated points.
        availableCPUNames = null;
        cpuNameByIndex.Clear();
        totalPointsByPlayerId.Clear();
    }

    // --- CPU identity persistence (see class header comment) ---

    public void EnsureCPUNamePoolInitialized(List<string> allNames)
    {
        if (availableCPUNames == null)
        {
            availableCPUNames = new List<string>(allNames ?? new List<string>());
        }
    }

    public string GetOrAssignCPUName(int cpuIndex, string fallbackName)
    {
        if (cpuNameByIndex.TryGetValue(cpuIndex, out string existing))
        {
            return existing;
        }

        string chosen = fallbackName;
        if (availableCPUNames != null && availableCPUNames.Count > 0)
        {
            int nameIndex = UnityEngine.Random.Range(0, availableCPUNames.Count);
            chosen = availableCPUNames[nameIndex];
            availableCPUNames.RemoveAt(nameIndex);
        }

        cpuNameByIndex[cpuIndex] = chosen;
        return chosen;
    }

    // --- Trophy points (see class header comment) ---

    public int GetPointsForPosition(int position)
    {
        int index = position - 1;
        return (index >= 0 && index < pointsTable.Count) ? pointsTable[index] : 0;
    }

    public void AddPointsForPlayer(string playerId, int points)
    {
        if (!totalPointsByPlayerId.ContainsKey(playerId))
        {
            totalPointsByPlayerId[playerId] = 0;
        }
        totalPointsByPlayerId[playerId] += points;
    }

    public int GetTotalPointsForPlayer(string playerId)
    {
        return totalPointsByPlayerId.TryGetValue(playerId, out int points) ? points : 0;
    }
}
