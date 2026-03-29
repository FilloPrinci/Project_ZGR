using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class GlobalInputManager : MonoBehaviour
{
    public static GlobalInputManager Instance { get; private set; }

    public int playerAmount = 1;
    public GameObject playerInputPrefab;

    private List<GameObject> playerInputList;

    private RaceManager raceManager;
    private GameSettings gameSettings;

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

        
    }

    private void Start()
    {
        raceManager = RaceManager.Instance;
        gameSettings = GameSettings.Instance;

        if (raceManager != null)
        {
            playerAmount = raceManager.GetHumanPlayersAmount();
        }

        InstantiatePlayerInputs(playerAmount);
    }

    void InstantiatePlayerInputs(int playerAmount) {

        playerInputList = new List<GameObject>();

        bool singleKeyboardPlayer = false;

        if(gameSettings != null && 
            gameSettings.inputMode == InputMode.KeyboardOnly && 
            playerAmount > 1)
        {
            singleKeyboardPlayer = true;
        }

        // instantiate player input prefabs for each player
        for (int i = 0; i < playerAmount; i++)
        {
            GameObject newPlayerInput = Instantiate(playerInputPrefab);
            playerInputList.Add(newPlayerInput);
            newPlayerInput.GetComponent<PlayerInputHandler>().SetPlayerIndex(i); // Set player index
            PlayerInput playerInputSettings = newPlayerInput.GetComponent<PlayerInput>();
            if(playerInputSettings != null)
            {
                if(singleKeyboardPlayer && i > 0)
                {
                    playerInputSettings.DeactivateInput();
                }
                else
                {
                    if (gameSettings != null)
                    {
                        if (gameSettings.inputMode == InputMode.GamepadOnly)
                        {
                            playerInputSettings.SwitchCurrentControlScheme("Gamepad", Gamepad.all[i]);
                        }
                        else if (gameSettings.inputMode == InputMode.KeyboardOnly)
                        {
                            playerInputSettings.SwitchCurrentControlScheme("Keyboard&Mouse", Keyboard.current);
                        }
                    }
                }


            }
        }
    }

    public void RemovePlayerInputInstances()
    {
        foreach (GameObject playerInput in playerInputList)
        {
            Destroy(playerInput);
        }
        playerInputList.Clear();
    }

    public GameObject GetPlayerInput(int playerIndex)
    {
        return playerInputList[playerIndex];
    }

    public bool IsIndexValid(int index)
    {
        return index >= 0 && index < playerInputList.Count;
    }

    public void GlobalSteer(InputAction.CallbackContext context, int index)
    {
        if(IsIndexValid(index))
        {
            playerInputList[index].GetComponent<PlayerInputHandler>().OnSteer(context);
        }
    }

    public void GlobalAccelerate(InputAction.CallbackContext context, int index)
    {
        if (IsIndexValid(index))
        {
            playerInputList[index].GetComponent<PlayerInputHandler>().OnAccelerate(context);
        }
        
    }

    public void GlobalStartPause(InputAction.CallbackContext context, int index)
    {
        if (IsIndexValid(index))
        {
            playerInputList[index].GetComponent<PlayerInputHandler>().OnSkip(context);
        }
    }
}
