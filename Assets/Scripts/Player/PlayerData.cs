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
}
