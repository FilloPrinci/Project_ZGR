using System.Collections.Generic;
using UnityEngine;



public class RaceSettings : MonoBehaviour
{
    public static RaceSettings Instance { get; private set; }

    public int inputPlayersAmount = 1;
    public int totalPlayersAmount = 10;
    public int laps = 3;
    
    public List<PlayerData> cpuPlayerDataList;
    public List<GameObject> veichlePrefabList;
    public int defaultVeichleIndex = 0;
    public int defaultRaceTrackIndex = 0;

    private SceneReferences sceneReferences;
    private List<PlayerData> inputPlayerDataList;
    private RaceMode selectedRaceMode = RaceMode.Test;
    private string selectedRaceTrack;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
                    }
        else
        {
            Debug.LogError("Duplicate RaceSettings detected. Destroying extra instance.");
            Destroy(gameObject); // Assicura che ci sia solo un RaceSettings
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sceneReferences = SceneReferences.Instance;
        inputPlayerDataList = new List<PlayerData>();
        selectedRaceTrack = sceneReferences.raceTrackSceneList[defaultRaceTrackIndex];

        if (veichlePrefabList.Count == 0)
        {
            Debug.LogError("[RaceSettings] ERROR: veichlePrefabList is empty");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public RaceMode GetSelectedRaceMode()
    {
        return selectedRaceMode;
    }

    public string GetSelectedRaceTrack()
    {
        return selectedRaceTrack;
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
        PlayerData inputPlayerData = new PlayerData("Player1", veichlePrefabList[defaultVeichleIndex], InputIndex.HID0);
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
            GameObject playrVeichle = veichlePrefabList[defaultVeichleIndex];
            InputIndex playerInputIndex = (InputIndex)i;
            PlayerData inputPlayerData = new PlayerData(playerName, playrVeichle, playerInputIndex);

            inputPlayerDataList.Add(inputPlayerData);
        }
    }

    public void OnVeichleSelect(int playerIndex, int veichleIndex)
    {
        inputPlayerDataList[playerIndex].veichlePrefab = veichlePrefabList[veichleIndex];
    }

    public void OnRaceTrackSelect(int trackIndex)
    {
        selectedRaceTrack = sceneReferences.raceTrackSceneList[trackIndex];
    }

    public void SetSelectedVeichleForPlayer(int playerIndex, GameObject veichlePrefab)
    {
        inputPlayerDataList[playerIndex].veichlePrefab = veichlePrefab;
        Debug.Log("[RaceSettings] INFO: player " + inputPlayerDataList[playerIndex].name + " has selected veichle " + veichlePrefab.name);
    }
}
