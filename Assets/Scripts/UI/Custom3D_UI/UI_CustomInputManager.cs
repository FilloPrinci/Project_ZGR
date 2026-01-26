using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_CustomInputManager : MonoBehaviour
{
    public GameObject playerInputPrefab;
    public bool LogInput = false;

    private int maxPlayers = 4;
    private List<GameObject> playerInputManagers;
    private UI_3D_Manager UI_3D_Manager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UI_3D_Manager = GetComponent<UI_3D_Manager>();

        if(UI_3D_Manager != null)
        {
            playerInputManagers = new List<GameObject>();

            for (int i = 0; i < maxPlayers; i++)
            {
                GameObject inputManager = Instantiate(playerInputPrefab, transform);
                inputManager.name = "PlayerInputManager_" + i;
                inputManager.GetComponent<UI_CustomPlayerInput>().playerIndex = i;
                inputManager.GetComponent<UI_CustomPlayerInput>().UI_InputManager = this.gameObject;
                inputManager.GetComponent<UI_CustomPlayerInput>().Initialize();
                playerInputManagers.Add(inputManager);
            }
        }
        else
        {
            Debug.LogError("[UI_CustomInputManager] ERROR: UI_3D_Manager component not found on the GameObject.");
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LogCustomInput(string message)
    {
        if (LogInput)
        {
            Debug.Log(message);
        }
    }

    public void OnSelectRight(int playerIndex)
    {
        LogCustomInput("Player " + playerIndex + " selected right");
        UI_3D_Manager.ManageSelectRight(playerIndex);
    }

    public void OnSelectLeft(int playerIndex)
    {
        LogCustomInput("Player " + playerIndex + " selected left");
        UI_3D_Manager.ManageSelectLeft(playerIndex);
    }

    public void OnConfirmSelection(int playerIndex)
    {
        LogCustomInput("Player " + playerIndex + " confirmed selection");
        UI_3D_Manager.ManageConfirmSelection(playerIndex);
    }

    public void OnCancelSelection(int playerIndex)
    {
        LogCustomInput("Player " + playerIndex + " canceled selection");
        UI_3D_Manager.ManageBackSelection(playerIndex);
    }
}
