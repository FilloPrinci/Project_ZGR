using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CPUInputHandlerManager : MonoBehaviour
{
    public static CPUInputHandlerManager Instance { get; private set; }

    private int cpuPlayerAmount = 0;

    private List<PlayerInputHandler> cpuInputHandlers = new List<PlayerInputHandler>();
    private RaceManager raceManager;

    public int GetCPUPlayerAmount()
    {
        return cpuPlayerAmount;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Duplicate CPUInputHandlerManager detected. Destroying extra instance.");
            Destroy(gameObject); // Assicura che ci sia solo un CPUInputHandlerManager
        }
        
    }

    void Start()
    {
        raceManager = RaceManager.Instance;

        if (raceManager != null) {
            cpuPlayerAmount = raceManager.cpuPlayersAmount;
            Debug.Log("[CPUInputHandlerManager] found " + cpuPlayerAmount + " CPU players from RaceManager");

            if(cpuPlayerAmount > 0)
            {
                // Initialize CPU input handlers
                for (int i = 0; i < cpuPlayerAmount; i++)
                {
                    GameObject cpuInputObject = new GameObject($"CPUInputHandler_{i}");
                    cpuInputObject.AddComponent<PlayerInputHandler>(); // Add PlayerInputHandler component to the GameObject
                    PlayerInputHandler cpuInputHandler = cpuInputObject.GetComponent<PlayerInputHandler>();
                    cpuInputHandler.SetPlayerIndex(i); // Set player index for CPU input handler
                    cpuInputHandlers.Add(cpuInputHandler);
                }
            }
        }
    }

    public PlayerInputHandler GetCPUInput(int cpuIndex)
    {
        return cpuInputHandlers[cpuIndex];
    }
}
