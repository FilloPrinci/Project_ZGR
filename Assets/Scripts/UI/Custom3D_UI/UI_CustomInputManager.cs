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
    private GameSettings _gameSettings;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _gameSettings = GameSettings.Instance;
        if (_gameSettings == null)
        {
            Debug.LogError("[UI_CustomInputManager] ERROR: GameSettings instance not found.");
        }

        UI_3D_Manager = GetComponent<UI_3D_Manager>();

        if(UI_3D_Manager != null)
        {
            playerInputManagers = new List<GameObject>();

            int playersAmount = GetInputAmount();
            maxPlayers = playersAmount;

            for (int i = 0; i < maxPlayers; i++)
            {
                GameObject inputManager = Instantiate(playerInputPrefab, transform);
                inputManager.name = "PlayerInputManager_" + i;

                if (_gameSettings.inputMode == InputMode.GamepadOnly)
                {
                    inputManager.GetComponent<PlayerInput>().SwitchCurrentControlScheme("Gamepad", Gamepad.all[i]);
                    Debug.Log("Player " + i + " assigned to gamepad: " + Gamepad.all[i].displayName);
                }
                else if (_gameSettings.inputMode == InputMode.KeyboardOnly)
                {
                    inputManager.GetComponent<PlayerInput>().SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current);
                    Debug.Log("Player " + i + " is using Keyboard");
                }

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

    private int GetInputAmount()
    {
        int inputAmount = 0;


        int gamepadAmount = Gamepad.all.Count;

        if (gamepadAmount > 0)
        {
            inputAmount = gamepadAmount;
        }
        else
        {
            inputAmount = 1;
        }

        return inputAmount;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateInputSettings()
    {
        if (playerInputManagers != null)
        {
            foreach (GameObject inputManager in playerInputManagers)
            {
                if (_gameSettings.inputMode == InputMode.GamepadOnly)
                {
                    inputManager.GetComponent<PlayerInput>().SwitchCurrentControlScheme("Gamepad");
                }
                else if (_gameSettings.inputMode == InputMode.KeyboardOnly)
                {
                    inputManager.GetComponent<PlayerInput>().SwitchCurrentControlScheme("Keyboard&Mouse");
                }
            }
        }
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
    public void OnSelectUp(int playerIndex)
    {
        LogCustomInput("Player " + playerIndex + " selected up");
        UI_3D_Manager.ManageSelectUp(playerIndex);
    }

    public void OnSelectDown(int playerIndex)
    {
        LogCustomInput("Player " + playerIndex + " selected down");
        UI_3D_Manager.ManageSelectDown(playerIndex);
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
