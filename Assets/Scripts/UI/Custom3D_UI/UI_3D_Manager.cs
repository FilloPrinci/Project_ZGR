using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


enum SlectionDirection
{
    Left,
    Right,
    Up,
    Down,
}

public class UI_3D_Manager : MonoBehaviour
{
    public static UI_3D_Manager Instance { get; private set; }
    
    [Header("Overlay")]
    public TextMeshProUGUI GroupName;
    public TextMeshProUGUI SelectionName;

    [Header("Settings")]
    public UI_GroupComponent Start_UI_GroupComponent;
    public float backgroundSpacing = 1f;
    public float panelSpacing = 2.0f;
    public float moveSpeed = 5.0f;
    public float panelScaleMultiplier;
    public float panelScaleSpeed;
    public float iconScaleMultiplier;
    public float iconScaleSpeed;

    [Header("Camera")]
    public GameObject mainCamera;
    public float cameraMoveSpeed = 5f;
    public float cameraRotateSpeed = 5f;
    public Transform cameraDefaultPosition;
    public Transform cameraVeichleSelectionPosition;


    [Header("Veichle Selection")]
    public GameObject veichleSelectionPrefab;
    public List<Transform> veichleSelectionPositionList;
    

    public bool debugMode = false;

    private UI_GroupComponent Current_UI_GroupCompoment;
    private List<UI_GroupComponent> UI_GroupComponent_Stack;
    private List<GameObject> veichleSelectorInstanceList;

    private RaceSettings _raceSettings;

    private Transform desideredCameraPosition;
    private float deltaTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Duplicate UI_3D_Manager detected. Destroying extra instance.");
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Start_UI_GroupComponent != null)
        {
            Current_UI_GroupCompoment = Start_UI_GroupComponent;
            Current_UI_GroupCompoment.Setup(panelSpacing, moveSpeed, panelScaleMultiplier, panelScaleSpeed, iconScaleMultiplier, iconScaleSpeed );
            Current_UI_GroupCompoment.InstantiateGroupGraphics();
            Current_UI_GroupCompoment.InitializeGroupLogic();

            UI_GroupComponent_Stack = new List<UI_GroupComponent>();
            UpdateOverlay();
        }

        _raceSettings = RaceSettings.Instance;
        if(_raceSettings == null)
        {
            Debug.LogError("[UI_3D_Manager] RaceSettings instance not found!");
        }

        if(mainCamera != null && cameraDefaultPosition != null)
        {
            desideredCameraPosition = cameraDefaultPosition;

            UpdateCameraPosition();
        }
    }
    // Update is called once per frame
    void Update()
    {
        deltaTime = Time.deltaTime;

        if (debugMode)
        {
            Start_UI_GroupComponent.Setup(panelSpacing, moveSpeed, panelScaleMultiplier, panelScaleSpeed, iconScaleMultiplier, iconScaleSpeed);
        }   
    }

    private void LateUpdate()
    {
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        if(mainCamera != null)
        {
            mainCamera.transform.position = Utils.ExpDecay(mainCamera.transform.position, desideredCameraPosition.position, cameraMoveSpeed, deltaTime);
            mainCamera.transform.rotation = Utils.ExpDecay(mainCamera.transform.rotation, desideredCameraPosition.rotation, cameraRotateSpeed, deltaTime);
        }
    }

    public void NavigateTo(UI_GroupComponent nextGroupComponent)
    {
        Debug.Log("[NavigateTo] " + nextGroupComponent.name);

        // push current group back and in the stack
        Current_UI_GroupCompoment.MoveBack(backgroundSpacing);
        UI_GroupComponent_Stack.Add(Current_UI_GroupCompoment);

        // show new group
        Current_UI_GroupCompoment = nextGroupComponent;
        Current_UI_GroupCompoment.Setup(panelSpacing, moveSpeed, panelScaleMultiplier, panelScaleSpeed, iconScaleMultiplier, iconScaleSpeed);
        Current_UI_GroupCompoment.InstantiateGroupGraphics();
        Current_UI_GroupCompoment.InitializeGroupLogic();
    }

    public void ManageSelectRight(int playerIndex)
    {
        Current_UI_GroupCompoment.SelectRight(playerIndex);
        UpdateOverlay();
    }

    public void ManageSelectLeft(int playerIndex)
    {
        Current_UI_GroupCompoment.SelectLeft(playerIndex);
        UpdateOverlay();
    }

    public void ManageConfirmSelection(int playerIndex)
    {
        Current_UI_GroupCompoment.ConfirmSelection(playerIndex);
        UpdateOverlay();
    }

    public void ManageBackSelection(int playerIndex)
    {
        if(UI_GroupComponent_Stack.Count != 0)
        {
            // Remove current group
            Current_UI_GroupCompoment.RemoveGroupGraphics();

            // Show last one from stack
            UI_GroupComponent lastUI_GroupComponent = UI_GroupComponent_Stack[UI_GroupComponent_Stack.Count - 1];
            Current_UI_GroupCompoment = lastUI_GroupComponent;
            Current_UI_GroupCompoment.BackFromSelection(playerIndex);
            Current_UI_GroupCompoment.MoveForward(backgroundSpacing);

            // update Stack
            UI_GroupComponent_Stack.RemoveAt(UI_GroupComponent_Stack.Count - 1);
        }
        else
        {
            Debug.Log("[ManageBackSelection] can't go back, no UI_Group_Component found in stack ");
        }

        UpdateOverlay();
    }

    void UpdateOverlay()
    {
        GroupName.text = Current_UI_GroupCompoment.GroupName;
        SelectionName.text = Current_UI_GroupCompoment.GetCurrentSelectionName();
    }

    public void StartVeichleSelection(int playersAmount = 1)
    {

        if(_raceSettings != null)
        {
            if(playersAmount <2)
            {
                _raceSettings.OnSingleplayerRaceModeSelect();
                _raceSettings.OnSinglePlayerSelect();
            }
            else
            {
                _raceSettings.OnMultiplayerRaceModeSelect();
                _raceSettings.OnMultiplayerAmountSelect(playersAmount);
            }
            
        }
        else
        {
            Debug.LogError("[UI_3D_Manager] RaceSettings instance not found!");
        }

        // Hide GroupComponent
        Current_UI_GroupCompoment.MoveBack(backgroundSpacing);
        UI_GroupComponent_Stack.Add(Current_UI_GroupCompoment);

        // Show Veichle selection

        if(veichleSelectorInstanceList == null)
        {
            veichleSelectorInstanceList = new List<GameObject>();
        }
        else
        {
            if (veichleSelectorInstanceList.Count > 0)
            {
                veichleSelectorInstanceList.Clear();
            }
        }
        
        for (int i = 0; i < playersAmount; i++)
        {
            veichleSelectorInstanceList.Add(Instantiate(veichleSelectionPrefab, veichleSelectionPositionList[i]));
        }

        // Move Camera
        desideredCameraPosition = cameraVeichleSelectionPosition;
    }
}
