using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class GlobalInputManager : MonoBehaviour
{
    public static GlobalInputManager Instance { get; private set; }

    public int playerAmount = 1;
    public GameObject playerInputPrefab;

    private List<GameObject> playerInputList;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Duplicate GlobalInputManager detected. Destroying extra instance.");
            Destroy(gameObject); // Assicura che ci sia solo un GlobalInputManager
        }

        InstantiatePlayerInputs(playerAmount);
    }

    void InstantiatePlayerInputs(int playerAmount) {

        playerInputList = new List<GameObject>();

        // instantiate player input prefabs for each player
        for (int i = 0; i < playerAmount; i++)
        {
            GameObject newPlayerInput = Instantiate(playerInputPrefab);
            playerInputList.Add(newPlayerInput);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject GetPlayerInput(int playerIndex)
    {
        return playerInputList[playerIndex];
    }
}
