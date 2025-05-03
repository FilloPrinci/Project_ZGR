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
    private string id;
    public string name;
    public GameObject veichlePrefab;
    public InputIndex playerInputIndex;

    public PlayerData(string name, GameObject playerVeichle, InputIndex playerInputIndex)
    {
        this.name = name;
        this.veichlePrefab = playerVeichle;
        this.playerInputIndex = playerInputIndex;
        this.id = Guid.NewGuid().ToString();
    }

    public string getID()
    {
        return this.id;
    }
}
