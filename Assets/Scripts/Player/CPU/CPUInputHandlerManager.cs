using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CPUInputHandlerManager : MonoBehaviour
{
    public static CPUInputHandlerManager Instance { get; private set; }

    public int cpuPlayerAmount = 1;

    private List<PlayerInputHandler> cpuInputHandlers = new List<PlayerInputHandler>();
    private RaceManager raceManager;

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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        raceManager = RaceManager.Instance;

        if (raceManager != null) {
            cpuPlayerAmount = raceManager.cpuPlayersAmount;
            Debug.Log("[CPUInputHandlerManager] found " + cpuPlayerAmount + " CPU players from RaceManager");
        }
        

        // Initialize CPU input handlers
        for (int i = 0; i < cpuPlayerAmount; i++)
        {
            GameObject cpuInputObject = new GameObject($"CPUInputHandler_{i}");
            cpuInputObject.AddComponent<PlayerInputHandler>(); // Add PlayerInputHandler component to the GameObject
            PlayerInputHandler cpuInputHandler = cpuInputObject.GetComponent<PlayerInputHandler>();
            cpuInputHandler.SetPlauerIndex(i); // Set player index for CPU input handler
            cpuInputHandlers.Add(cpuInputHandler);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public PlayerInputHandler GetCPUInput(int cpuIndex)
    {
        return cpuInputHandlers[cpuIndex];
    }
}
