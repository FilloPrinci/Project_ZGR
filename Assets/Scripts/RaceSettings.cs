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

    // CPU driving skill persistence: a CPU slot (cpuIndex) keeps the same skill level (0-10)
    // across races within the same session/trophy, same lifecycle as the name above. The whole
    // batch is rolled once (EnsureCPUSkillPoolInitialized), not lazily per CPU, because the
    // "1-3 CPUs at max skill" guarantee is a constraint over the whole roster, not a single slot.
    private Dictionary<int, int> cpuSkillByIndex = new Dictionary<int, int>();
    private bool cpuSkillPoolInitialized = false;

    [Header("CPU Skill")]
    // Chance that a non-guaranteed CPU rolls a skill level above 5 (6-10) instead of 0-5,
    // indexed by GlobalDifficulty (easy=0, normal=1, hard=2). Tune in Inspector.
    public List<float> highSkillChanceByDifficulty = new List<float> { 0.2f, 0.5f, 0.8f };

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
        cpuSkillByIndex.Clear();
        cpuSkillPoolInitialized = false;
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

    // --- CPU skill persistence (see field comment above) ---

    // Rolls skill levels for a batch of CPUs (cpuIndex in [startCpuIndex, startCpuIndex+cpuCount)):
    // always 1-3 of them (capped to cpuCount) get the max skill (10), the rest roll 6-10 with
    // probability highSkillChance or 0-5 otherwise. Static/pure so RaceManager's no-RaceSettings
    // fallback (testing the race scene directly in the Editor) can reuse the exact same rule.
    public static Dictionary<int, int> GenerateCPUSkillBatch(int startCpuIndex, int cpuCount, float highSkillChance)
    {
        Dictionary<int, int> result = new Dictionary<int, int>();
        if (cpuCount <= 0) return result;

        int guaranteedTopSkillCount = Mathf.Min(UnityEngine.Random.Range(1, 4), cpuCount); // 1-3 inclusive
        HashSet<int> topSkillOffsets = new HashSet<int>();
        while (topSkillOffsets.Count < guaranteedTopSkillCount)
        {
            topSkillOffsets.Add(UnityEngine.Random.Range(0, cpuCount));
        }

        for (int offset = 0; offset < cpuCount; offset++)
        {
            int skill;
            if (topSkillOffsets.Contains(offset))
            {
                skill = 10;
            }
            else if (UnityEngine.Random.value < highSkillChance)
            {
                skill = UnityEngine.Random.Range(6, 11); // 6-10 ("above 5")
            }
            else
            {
                skill = UnityEngine.Random.Range(0, 6); // 0-5
            }

            result[startCpuIndex + offset] = skill;
        }

        return result;
    }

    private float GetHighSkillChanceForCurrentDifficulty()
    {
        int index = (int)selectedDifficulty;
        if (highSkillChanceByDifficulty != null && index >= 0 && index < highSkillChanceByDifficulty.Count)
        {
            return highSkillChanceByDifficulty[index];
        }
        return 0.5f;
    }

    // Rolls the whole trophy's CPU skill batch once (see class field comment); later calls in
    // the same session/trophy are a no-op, exactly like EnsureCPUNamePoolInitialized above.
    public void EnsureCPUSkillPoolInitialized(int startCpuIndex, int cpuCount)
    {
        if (cpuSkillPoolInitialized) return;
        cpuSkillPoolInitialized = true;

        Dictionary<int, int> batch = GenerateCPUSkillBatch(startCpuIndex, cpuCount, GetHighSkillChanceForCurrentDifficulty());
        foreach (KeyValuePair<int, int> pair in batch)
        {
            cpuSkillByIndex[pair.Key] = pair.Value;
        }
    }

    public int GetOrAssignCPUSkill(int cpuIndex)
    {
        if (cpuSkillByIndex.TryGetValue(cpuIndex, out int existing))
        {
            return existing;
        }

        // Not covered by the batch rolled in EnsureCPUSkillPoolInitialized (e.g. this session's
        // CPU count grew between races) - roll a single difficulty-biased value as a fallback.
        // The "1-3 at max skill" guarantee only applies to a full batch roll, not a single slot.
        float highSkillChance = GetHighSkillChanceForCurrentDifficulty();
        int skill = UnityEngine.Random.value < highSkillChance
            ? UnityEngine.Random.Range(6, 11)
            : UnityEngine.Random.Range(0, 6);

        cpuSkillByIndex[cpuIndex] = skill;
        return skill;
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
