using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string name;
    public GameObject playerVeichle;
    public int playerInputIndex;

    public PlayerData(string name, GameObject playerVeichle, int playerInputIndex)
    {
        this.name = name;
        this.playerVeichle = playerVeichle;
        this.playerInputIndex = playerInputIndex;
    }
}
