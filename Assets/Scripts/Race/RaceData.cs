using System;
using System.Collections.Generic;
using UnityEngine;

public class TimeData
{
    public int lap;
    public float time;

    public TimeData(int lap, float time)
    {
        this.lap = lap;
        this.time = time;
    }
}

public class PlayerRaceData
{
    public PlayerData playerData;
    public int position;
    public string time;
    public int currentSectorIndex;
    public int nextCheckpointIndex;
    public float currentCheckpointDistance;
    public int currentLap;
    public TimeData currentLapTime;
    public float startTime;
    public List<TimeData> lapTimes = new List<TimeData>();
    public bool inRace = true;

    public PlayerRaceData(PlayerData playerData, int position, string time, int currentSectorIndex, int nextCheckpointIndex, float currentCheckpointDistance, int currentLap, bool inRace)
    {
        this.playerData = playerData;
        this.position = position;
        this.time = time;
        this.currentSectorIndex = currentSectorIndex;
        this.nextCheckpointIndex = nextCheckpointIndex;
        this.currentCheckpointDistance = currentCheckpointDistance;
        this.currentLap = currentLap;
        this.lapTimes = new List<TimeData>();
        this.currentLapTime = new TimeData(0, 0);
        this.startTime = 0f;
        this.inRace = inRace;
    }

    public bool RaceCompleted(int maxLaps) {
        bool raceCompleted = false;
        if (currentLap > maxLaps)
        {
            raceCompleted = true;
        }

        return raceCompleted;
    }

    public string GetCurrentLapTime()
    {
        return TimeToString(this.currentLapTime.time);
    }

    public string GetTotalTime()
    {
        float totalTime = 0f;
        foreach (TimeData lapTime in lapTimes)
        {
            totalTime += lapTime.time;
        }
        return TimeToString(totalTime);
    }

    public string GetBestLapTime()
    {
        if (lapTimes.Count == 0)
        {
            return TimeToString(0); ; // No laps completed
        }
        TimeData bestLap = lapTimes[0];
        foreach (TimeData lapTime in lapTimes)
        {
            if (lapTime.time < bestLap.time)
            {
                bestLap = lapTime;
            }
        }
        return TimeToString(bestLap.time);
    }

    public string TimeToString(float time)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        return string.Format("{0:D2}:{1:D2}:{2:D3}", (int)timeSpan.TotalMinutes, timeSpan.Seconds, timeSpan.Milliseconds);
    }
}

public class RaceData
{
    public List<PlayerRaceData> playerRaceDataList;

    private List<PlayerRaceData> positionOrderedPlayerRaceDataList;

    private List<PlayerRaceData> finalResultPlayerRaceDataList;

    private float startTime;

    public RaceData(List<PlayerRaceData> playerRaceDataList)
    {
        this.playerRaceDataList = playerRaceDataList;
       
    }

    public void StartRace()
    {
        startTime = Time.time;

        if(finalResultPlayerRaceDataList != null)
        {
            finalResultPlayerRaceDataList.Clear();
        }
        else
        {
            finalResultPlayerRaceDataList = new List<PlayerRaceData>();
        }

            foreach (PlayerRaceData playerRaceData in this.playerRaceDataList)
            {
                playerRaceData.startTime = startTime;
            }

    }

    public void RefreshPositions()
    {
        // Step 1: Sort the entire list based on the criteria: Lap → Sector → Distance
        positionOrderedPlayerRaceDataList = new List<PlayerRaceData>(playerRaceDataList);

        positionOrderedPlayerRaceDataList.Sort((a, b) =>
        {
            // First, sort by lap (descending)
            int lapComparison = b.currentLap.CompareTo(a.currentLap);
            if (lapComparison != 0)
                return lapComparison;

            // Then, sort by sector index (descending)
            int sectorComparison = b.currentSectorIndex.CompareTo(a.currentSectorIndex);
            if (sectorComparison != 0)
                return sectorComparison;

            // Finally, sort by distance to the next checkpoint (ascending)
            return a.currentCheckpointDistance.CompareTo(b.currentCheckpointDistance);
        });

        // Step 2: Assign positions based on the sorted list
        for (int i = 0; i < positionOrderedPlayerRaceDataList.Count; i++)
        {
            positionOrderedPlayerRaceDataList[i].position = i + 1;
        }

    }

    public void UpdatePlayerRaceDataTimings()
    {
        float currentTime = Time.time;
        foreach (PlayerRaceData playerRaceData in playerRaceDataList)
        {
            float playerTime = currentTime - playerRaceData.startTime;
            playerRaceData.currentLapTime.time = playerTime;
        }
    }

    public void SetLapTimeForPlayer(int playerDataToUpdateIndex)
    {
        float currentTime = Time.time;

        PlayerRaceData playerRaceData = playerRaceDataList[playerDataToUpdateIndex];
        float playerTime = currentTime - playerRaceData.startTime;
        playerRaceData.lapTimes.Add(new TimeData(playerRaceData.currentLap, playerTime));
        playerRaceData.startTime = currentTime; // Reset start time for the next lap

    }

    public PlayerRaceData GetPlayerRaceDataByID(string id)
    {
        PlayerRaceData playerRaceData = playerRaceDataList.Find(p => p.playerData.name == id);
        return playerRaceData;
    }
    public List<string> GetRaceDataAsLines()
    {
        List<string> lines = new List<string>();
        foreach (PlayerRaceData raceData in playerRaceDataList)
        {
            string line = $"{raceData.position}\t{raceData.playerData.name}\t{raceData.currentLap}\t{raceData.nextCheckpointIndex}\t{raceData.currentCheckpointDistance}";
            lines.Add(line);
        }


        return lines;
    }

    public void AddFinalResultForPlayerRaceData(PlayerRaceData playerRaceData)
    {
        finalResultPlayerRaceDataList.Add(playerRaceData);
    }

    public List<PlayerRaceData> GetFinalPlayerRaceDataList()
    {
        return finalResultPlayerRaceDataList;
    }
}
