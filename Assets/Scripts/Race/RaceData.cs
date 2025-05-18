using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRaceData
{
    public PlayerData playerData;
    public int position;
    public string time;
    public int currentSectorIndex;
    public int nextCheckpointIndex;
    public float currentCheckpointDistance;
    public int currentLap;

    public PlayerRaceData(PlayerData playerData, int position, string time, int currentSectorIndex, int nextCheckpointIndex, float currentCheckpointDistance, int currentLap)
    {
        this.playerData = playerData;
        this.position = position;
        this.time = time;
        this.currentSectorIndex = currentSectorIndex;
        this.nextCheckpointIndex = nextCheckpointIndex;
        this.currentCheckpointDistance = currentCheckpointDistance;
        this.currentLap = currentLap;
    }

    public bool RaceCompleted(int maxLaps) {
        bool raceCompleted = false;
        if (currentLap > maxLaps)
        {
            raceCompleted = true;
        }

        return raceCompleted;
    }
}

public class RaceData
{
    public List<PlayerRaceData> playerRaceDataList;

    private List<PlayerRaceData> positionOrderedPlayerRaceDataList;

    public RaceData(List<PlayerRaceData> playerRaceDataList)
    {
        this.playerRaceDataList = playerRaceDataList;
       
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
}
