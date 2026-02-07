using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


enum SlectionDirection
{
    Left,
    Right,
    Up,
    Down,
}

enum SelectionPhase
{
    Menu,
    Veichle,
    Track
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

    private SelectionPhase selectionPhase = SelectionPhase.Menu;

    private UI_GroupComponent Current_UI_GroupCompoment;
    private List<UI_GroupComponent> UI_GroupComponent_Stack;
    private List<GameObject> veichleSelectorInstanceList;

    private RaceSettings _raceSettings;
    private GameSettings _gameSettings;

    private Vector3 desideredCameraPosition;
    private Quaternion desideredCameraRotation;
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

        _gameSettings = GameSettings.Instance;
        if(_gameSettings == null)
        {
            Debug.LogError("[UI_3D_Manager] GameSettings instance not found!");
        }

        if (mainCamera != null && cameraDefaultPosition != null)
        {
            desideredCameraPosition = cameraDefaultPosition.position;
            desideredCameraRotation = cameraDefaultPosition.rotation;

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
            mainCamera.transform.position = Utils.ExpDecay(mainCamera.transform.position, desideredCameraPosition, cameraMoveSpeed, deltaTime);
            mainCamera.transform.rotation = Utils.ExpDecay(mainCamera.transform.rotation,desideredCameraRotation, cameraRotateSpeed, deltaTime);
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
        if (selectionPhase == SelectionPhase.Menu)
        {
            if (Current_UI_GroupCompoment != null)
            {
                Current_UI_GroupCompoment.SelectRight(playerIndex);
                UpdateOverlay();
            }
        }
        else if (selectionPhase == SelectionPhase.Veichle) {
            if (veichleSelectorInstanceList.Count > 0)
            {
                GameObject playerVeichleSelector = veichleSelectorInstanceList[playerIndex];
                if (playerVeichleSelector != null) {
                    playerVeichleSelector.GetComponent<UI_3D_VeichleSelector>().SelectRight();
                }
            }
        }
        
        
    }

    public void ManageSelectLeft(int playerIndex)
    {
        if (selectionPhase == SelectionPhase.Menu)
        {
            if (Current_UI_GroupCompoment != null)
            {
                Current_UI_GroupCompoment.SelectLeft(playerIndex);
                UpdateOverlay();
            }
        }
        else if (selectionPhase == SelectionPhase.Veichle)
        {
            if (veichleSelectorInstanceList.Count > 0) {
                GameObject playerVeichleSelector = veichleSelectorInstanceList[playerIndex];
                if (playerVeichleSelector != null)
                {
                    playerVeichleSelector.GetComponent<UI_3D_VeichleSelector>().SelectLeft();
                }
            }
        }
    }

    public void ManageSelectUp(int playerIndex)
    {
        if (selectionPhase == SelectionPhase.Menu)
        {
            if (Current_UI_GroupCompoment != null)
            {
                Current_UI_GroupCompoment.SelectUp(playerIndex);
                UpdateOverlay();
            }
        }
    }

    public void ManageSelectDown(int playerIndex)
    {
        if (selectionPhase == SelectionPhase.Menu)
        {
            if (Current_UI_GroupCompoment != null)
            {
                Current_UI_GroupCompoment.SelectDown(playerIndex);
                UpdateOverlay();
            }
        }
    }


    public void ManageConfirmSelection(int playerIndex)
    {
        if (selectionPhase == SelectionPhase.Menu)
        {
            if (Current_UI_GroupCompoment != null)
            {
                Current_UI_GroupCompoment.ConfirmSelection(playerIndex);
                UpdateOverlay();
            }
        }
        else if (selectionPhase == SelectionPhase.Veichle)
        {
            if (veichleSelectorInstanceList.Count > 0)
            {
                GameObject playerVeichleSelector = veichleSelectorInstanceList[playerIndex];
                if (playerVeichleSelector != null)
                {
                    playerVeichleSelector.GetComponent<UI_3D_VeichleSelector>().ConfirmSelection();
                }
            }
        }
    }

    public void ManageBackSelection(int playerIndex)
    {
        if(selectionPhase == SelectionPhase.Menu)
        {
            if(Current_UI_GroupCompoment != null)
            {
                if (Current_UI_GroupCompoment.inSubMenu)
                {
                    Current_UI_GroupCompoment.BackFromSubMenu(playerIndex);
                    UpdateOverlay();
                    return;
                }
            }

            if (UI_GroupComponent_Stack.Count != 0)
            {
                // Remove current group
                if(Current_UI_GroupCompoment != null)
                {
                    Current_UI_GroupCompoment.RemoveGroupGraphics();
                }
                

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
        else if (selectionPhase == SelectionPhase.Veichle)
        {
            if (veichleSelectorInstanceList.Count > 0)
            {
                GameObject playerVeichleSelector = veichleSelectorInstanceList[playerIndex];
                if (playerVeichleSelector != null)
                {
                    playerVeichleSelector.GetComponent<UI_3D_VeichleSelector>().CancelSelection();
                }
            }
        }
    }

    public void ManageBackFromVeichleSelection(int playerIndex)
    {
        if (veichleSelectorInstanceList.Count > 0)
        {
            for (int i = 0; i < veichleSelectorInstanceList.Count; i++)
            {
                DestroyImmediate(veichleSelectorInstanceList[i]);
            }
            selectionPhase = SelectionPhase.Menu;
            ManageBackSelection(playerIndex);

            // Move Camera
            desideredCameraPosition = cameraDefaultPosition.position;
            desideredCameraRotation = cameraDefaultPosition.rotation;
        }

    }

    void UpdateOverlay()
    {
        if (selectionPhase == SelectionPhase.Menu) {
            GroupName.text = Current_UI_GroupCompoment.GroupName;
            SelectionName.text = Current_UI_GroupCompoment.GetCurrentSelectionName();
        }
        else
        {
            GroupName.text = selectionPhase.ToString();
            SelectionName.text = "";
        }
        
    }

    public void StartVeichleSelection(int playersAmount = 1)
    {

        if(_raceSettings != null)
        {
            _raceSettings.ResetSettings();

            if (playersAmount <2)
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
        Current_UI_GroupCompoment = null;

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

            GameObject veichleSelectionInstance = Instantiate(veichleSelectionPrefab, veichleSelectionPositionList[i]);
            veichleSelectionInstance.GetComponent<UI_3D_VeichleSelector>().SetMainCamera(mainCamera);
            veichleSelectionInstance.GetComponent<UI_3D_VeichleSelector>().playerIndex = i;
            veichleSelectorInstanceList.Add(veichleSelectionInstance);
        }

       
        Vector3 updatedVeichleCameraPosition = cameraVeichleSelectionPosition.position;
        updatedVeichleCameraPosition = CalculateCameraVeichlePosition(veichleSelectionPositionList[0].position, veichleSelectionPositionList[playersAmount - 1].position);

        // Move Camera
        desideredCameraPosition = updatedVeichleCameraPosition;
        desideredCameraRotation = cameraVeichleSelectionPosition.rotation;

        selectionPhase = SelectionPhase.Veichle;
    }

    public void OnVeichleSelectionReady()
    {
        // check if all players are ready to proceed
        bool canProceed = true;

        for (int i = 0; i < veichleSelectorInstanceList.Count; i++)
        {

            if (!veichleSelectorInstanceList[i].GetComponent<UI_3D_VeichleSelector>().selectionCompleted)
            {
                canProceed = false;
            }
        }

        // if all players are ready
        if (canProceed)
        {
            // TODO: select track
            // for now let's skip to race start
            string raceTracSceneName = _raceSettings.GetSelectedRaceTrack();
            SceneManager.LoadSceneAsync(raceTracSceneName);
        }
    }

    private Vector3 CalculateCameraVeichlePosition(Vector3 firstPosition, Vector3 lastPosition)
    {
        Vector3 origin = firstPosition;
        Vector3 avgPosition = (lastPosition - firstPosition) / 2;
        Vector3 result = new Vector3(cameraVeichleSelectionPosition.position.x + origin.x + avgPosition.x, cameraVeichleSelectionPosition.position.y, cameraVeichleSelectionPosition.position.z);
        return result;
    }
}
