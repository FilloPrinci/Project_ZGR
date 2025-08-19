using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CPUInputHandlerManager : MonoBehaviour
{
    public static CPUInputHandlerManager Instance { get; private set; }

    public int cpuPlayerAmount = 1;

    private List<PlayerInputHandler> cpuInputHandlers = new List<PlayerInputHandler>();

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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
