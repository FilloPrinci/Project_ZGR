using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GlobalDifficulty
{
    easy,
    normal,
    hard
}

[System.Serializable]
public class DifficultySettings
{
    public GlobalDifficulty globalDifficulty;
    [UnityEngine.Range(0, 3)]
    public int goFasterLevel = 1;
    [UnityEngine.Range(0, 3)]
    public int goSlowerLevel = 1;

    public DifficultySettings(GlobalDifficulty globalDifficulty, int bottomRubberbanding, int topRubberbanding)
    {
        this.globalDifficulty = globalDifficulty;
        this.goFasterLevel = bottomRubberbanding;
        this.goSlowerLevel = topRubberbanding;


    }
}

public class RaceDifficultyManager : MonoBehaviour
{

    public List<DifficultySettings>  difficultySettings;
    public GlobalDifficulty difficulty;

    public float managementRefreshRate = 1f;

    private RaceManager raceManager;
    private List<GameObject> allPlayerInstanceList;
    private List<GameObject> humanPlayerInstanceList;
    private List<GameObject> cpuPlayerInstanceList;
    private int bestPlayerPosition = 1;
    private DifficultySettings selectedDifficultySettings;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        selectedDifficultySettings = difficultySettings.Find(settings => settings.globalDifficulty == difficulty);

        raceManager = RaceManager.Instance;

        if(raceManager == null)
        {
            Debug.LogWarning("RaceDifficultyManager: No RaceManager instance found in the scene.");
        }
        else
        {
            if (raceManager.IsReady())
            {
                cpuPlayerInstanceList = GetCPUPlayerInstanceList();

                humanPlayerInstanceList = new List<GameObject>();
                foreach (GameObject playerInstance in allPlayerInstanceList)
                {
                    PlayerController playerController = playerInstance.GetComponent<PlayerController>();
                    if (playerController != null && playerController.IsHuman())
                    {
                        humanPlayerInstanceList.Add(playerInstance);
                    }
                }
                if (cpuPlayerInstanceList != null && cpuPlayerInstanceList.Count > 0)
                {
                    StartCoroutine(UpdateRubberbanding(managementRefreshRate));
                }
                else
                {
                    Debug.LogWarning("RaceDifficultyManager: RaceManager is not ready. Player instances may not be available yet.");
                }
            }
        }
    }

    List<GameObject> GetCPUPlayerInstanceList()
    {
        allPlayerInstanceList = raceManager.GetPlayerInstanceList();

        if (allPlayerInstanceList != null && allPlayerInstanceList.Count > 0)
        {
            List<GameObject> cpuPlayerInstanceList = new List<GameObject>();
            foreach (GameObject playerInstance in allPlayerInstanceList)
            {
                PlayerController playerController = playerInstance.GetComponent<PlayerController>();

                if (playerController != null && !playerController.IsHuman())
                {
                    cpuPlayerInstanceList.Add(playerInstance);
                }
            }
            return cpuPlayerInstanceList;
        }
        else
        {
            Debug.LogWarning("RaceDifficultyManager: No player instances found in the scene.");
            return new List<GameObject>();
        }
    }

    IEnumerator UpdateRubberbanding(float Hertz)
    {
        float timeInterval = 1f / Hertz;

        var wait = new WaitForSeconds(timeInterval);

        while (true)
        {
            if(raceManager.GetCurrentRacePhase() == RacePhase.Race)
            {
                // Update rubberbanding logic here based on the difficulty settings and player positions
                UpdateRubberbandingLogic();
            }
            
            yield return wait;
        }
    }

    void UpdateRubberbandingLogic()
    {
        // Implement rubberbanding logic based on the difficulty settings and player positions
        // This may involve adjusting the speed or performance of CPU players based on their position in race

        // OrderByPosition

        cpuPlayerInstanceList.Sort((a, b) =>
        {
            PlayerRaceData aRaceData = a.GetComponent<PlayerController>().GetCurrentRaceData();
            PlayerRaceData bRaceData = b.GetComponent<PlayerController>().GetCurrentRaceData();
            if (aRaceData != null && bRaceData != null)
            {
                return aRaceData.position.CompareTo(bRaceData.position);
            }
            else
            {
                return 0; // If either player instance does not have PlayerRaceData, consider them equal for sorting purposes
            }
        });

        // retrive player positions and apply rubberbanding adjustments based on the difficulty settings
        humanPlayerInstanceList.Sort((a, b) =>
        {
            PlayerRaceData aRaceData = a.GetComponent<PlayerController>().GetCurrentRaceData();
            PlayerRaceData bRaceData = b.GetComponent<PlayerController>().GetCurrentRaceData();
            if (aRaceData != null && bRaceData != null)
            {
                return aRaceData.position.CompareTo(bRaceData.position);
            }
            else
            {
                return 0; // If either player instance does not have PlayerRaceData, consider them equal for sorting purposes
            }
        });
        bestPlayerPosition = humanPlayerInstanceList[0].GetComponent<PlayerController>().GetCurrentPositionInRace();

        // If player is in the bottom positions, apply top rubberbanding level
        if (bestPlayerPosition > cpuPlayerInstanceList.Count / 2)
        {
            // Apply top rubberbanding level to CPU players
            foreach (GameObject cpuPlayer in cpuPlayerInstanceList)
            {
                // Adjust CPU player performance based on the top rubberbanding level
                if(cpuPlayer.GetComponent<PlayerController>().GetCurrentPositionInRace() < bestPlayerPosition)
                {
                    // CPU player is ahead of the best human player, apply top rubberbanding level so the CPU slow down
                    cpuPlayer.GetComponent<PlayerController>().SetRubberbandLevel(-1 * selectedDifficultySettings.goSlowerLevel);
                }
                else
                {
                    cpuPlayer.GetComponent<PlayerController>().SetRubberbandLevel(0); // No rubberbanding for CPU players behind the best human player
                }
            }
        }
        else
        {
            // If player is in the top positions, apply bottom rubberbanding level
            // Apply bottom rubberbanding level to CPU players
            foreach (GameObject cpuPlayer in cpuPlayerInstanceList)
            {
                // Adjust CPU player performance based on the top rubberbanding level
                if (cpuPlayer.GetComponent<PlayerController>().GetCurrentPositionInRace() > bestPlayerPosition)
                {
                    // CPU player is behind of the best human player, apply top rubberbanding level so the CPU gets faster
                    cpuPlayer.GetComponent<PlayerController>().SetRubberbandLevel(1 * selectedDifficultySettings.goFasterLevel);
                }
                else
                {
                    cpuPlayer.GetComponent<PlayerController>().SetRubberbandLevel(0); // No rubberbanding for CPU players ahead the best human player
                }
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
