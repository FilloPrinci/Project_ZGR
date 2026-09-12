using UnityEngine;
using System;
public enum InputIndex
{
    HID0,
    HID1,
    HID2,
    HID3,
    CPU
}
[System.Serializable]
public class PlayerData
{
    public string nameId;
    public string displayName;
    public GameObject vehiclePrefab;
    public InputIndex playerInputIndex;
    public int cpuIndex = -1;
    // CPU driving skill (0-10). Persisted per trophy/session in RaceSettings, keyed by cpuIndex
    // (see RaceSettings.GetOrAssignCPUSkill) — set once via SetSkillLevel when the PlayerData is
    // created in RaceManager.Start(). Unused for human players.
    public int skillLevel = 0;

    public PlayerData(string name, GameObject playerVehicle, InputIndex playerInputIndex, String displayName = null)
    {
        this.nameId = name;
        this.displayName = displayName;
        this.vehiclePrefab = playerVehicle;
        this.playerInputIndex = playerInputIndex;
    
        if(displayName == null)
        {
            this.displayName = "[" + name + "]";
        }
    }

    public void SetCPUIndex(int index)
    {
        cpuIndex = index;
    }

    public void SetSkillLevel(int level)
    {
        skillLevel = level;
    }
}
