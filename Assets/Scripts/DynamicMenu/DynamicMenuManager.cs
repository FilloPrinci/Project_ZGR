using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public enum MenuStepName
{
    PressStart,
    MainMenu,
    Settings,
    PlayModeSelection,
    MultiplayerSetup,
    SinglePlayerSetup,
    VeichleSelection
}

[System.Serializable]
public class MenuStep
{
    public MenuStepName StepName;
    public GameObject StepUI;
    public Transform StepCameraPosition;

    public MenuStep(MenuStepName stepName, Transform stepCameraPosition, GameObject stepUI)
    {
        StepName = stepName;
        StepCameraPosition = stepCameraPosition;
        StepUI = stepUI;
    }
}

public class DynamicMenuManager : MonoBehaviour
{
    public static DynamicMenuManager Instance { get; private set; }

    [SerializeField]
    public List<MenuStep> menuSteps;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Duplicate DynamicMenuManager detected. Destroying extra instance.");
            Destroy(gameObject); // Assicura che ci sia solo un DynamicMenuManager
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
}
