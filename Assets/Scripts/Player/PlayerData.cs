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
    public string name;
    public GameObject veichlePrefab;
    public InputIndex playerInputIndex;
    public int cpuIndex = -1;

    public PlayerData(string name, GameObject playerVeichle, InputIndex playerInputIndex)
    {
        this.name = name;
        this.veichlePrefab = playerVeichle;
        this.playerInputIndex = playerInputIndex;
    }

    public void SetCPUIndex(int index)
    {
        cpuIndex = index;
    }
}
