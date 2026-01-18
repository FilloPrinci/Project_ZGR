using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI_CustomInputManager : MonoBehaviour
{
    public GameObject playerInputPrefab;

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

    public void OnSelectRight(int playerIndex)
    {
        Debug.Log("Player " + playerIndex + " selected right");
        UI_3D_Manager.ManageSelectRight(playerIndex);
    }
}
