using NUnit.Framework;
using System.Collections.Generic;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Start_UI_GroupComponent != null)
        {
            Current_UI_GroupCompoment = Start_UI_GroupComponent;
            Current_UI_GroupCompoment.Setup(panelSpacing, moveSpeed, panelScaleMultiplier, panelScaleSpeed, iconScaleMultiplier, iconScaleSpeed );
            Current_UI_GroupCompoment.InstantiateGroupGraphics();
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

    public void ManageSelectRight(int playerIndex)
    {
        Current_UI_GroupCompoment.SelectRight(playerIndex);
    }

    public void ManageSelectLeft(int playerIndex)
    {
        Current_UI_GroupCompoment.SelectLeft(playerIndex);
    }

    public void ManageConfirmSelection(int playerIndex)
    {
        Current_UI_GroupCompoment.ConfirmSelection(playerIndex);
        Current_UI_GroupCompoment.MoveBack(backgroundSpacing);

        // TODO: Change Current_UI_GroupCompoment

    }

    public void ManageCancelSelection(int playerIndex)
    {
        Current_UI_GroupCompoment.RemoveGroupGraphics();

        // TODO: Change Current_UI_GroupCompoment

        Current_UI_GroupCompoment.BackFromSelection(playerIndex);
        Current_UI_GroupCompoment.MoveForward(backgroundSpacing);
    }
}
