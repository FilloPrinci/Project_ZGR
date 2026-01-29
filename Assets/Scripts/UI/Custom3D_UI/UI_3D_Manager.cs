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

    public bool debugMode = false;

    private UI_GroupComponent Current_UI_GroupCompoment;
    private List<UI_GroupComponent> UI_GroupComponent_Stack;

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
    }
    // Update is called once per frame
    void Update()
    {
        if(debugMode)
        {
            Start_UI_GroupComponent.Setup(panelSpacing, moveSpeed, panelScaleMultiplier, panelScaleSpeed, iconScaleMultiplier, iconScaleSpeed);
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
}
