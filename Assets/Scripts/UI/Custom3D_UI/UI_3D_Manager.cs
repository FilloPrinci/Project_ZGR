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
    public List<UI_GroupComponent> UI_ComponentGroupList;
    public int ActiveUIGroupIndex = 0;

    public float backgroundSpacing = 1f;

    public float panelSpacing = 2.0f;
    public float moveSpeed = 5.0f;
    public float panelScaleMultiplier;
    public float panelScaleSpeed;
    public float iconScaleMultiplier;
    public float iconScaleSpeed;

    public bool debugMode = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (UI_ComponentGroupList != null)
        {
            UI_ComponentGroupList[ActiveUIGroupIndex].Setup(panelSpacing, moveSpeed, panelScaleMultiplier, panelScaleSpeed, iconScaleMultiplier, iconScaleSpeed );
            UI_ComponentGroupList[ActiveUIGroupIndex].InstantiateGroupGraphics();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(debugMode)
        {
            UI_ComponentGroupList[ActiveUIGroupIndex].Setup(panelSpacing, moveSpeed, panelScaleMultiplier, panelScaleSpeed, iconScaleMultiplier, iconScaleSpeed);
        }
        
    }

    public void ManageSelectRight(int playerIndex)
    {
        UI_ComponentGroupList[ActiveUIGroupIndex].SelectRight(playerIndex);
    }

    public void ManageSelectLeft(int playerIndex)
    {
        UI_ComponentGroupList[ActiveUIGroupIndex].SelectLeft(playerIndex);
    }

    public void ManageConfirmSelection(int playerIndex)
    {
        UI_ComponentGroupList[ActiveUIGroupIndex].ConfirmSelection(playerIndex);

        if (ActiveUIGroupIndex < UI_ComponentGroupList.Count - 1)
        {
            ActiveUIGroupIndex++;
            UI_ComponentGroupList[ActiveUIGroupIndex].Setup(panelSpacing, moveSpeed, panelScaleMultiplier, panelScaleSpeed, iconScaleMultiplier, iconScaleSpeed);
            UI_ComponentGroupList[ActiveUIGroupIndex].InstantiateGroupGraphics();
        }

        
        for (int i = 0; i < ActiveUIGroupIndex; i++)
        {
            UI_ComponentGroupList[i].MoveBack(backgroundSpacing);
        }
        
        
    }

    public void ManageCancelSelection(int playerIndex)
    {
        if (ActiveUIGroupIndex > 0)
        {
            UI_ComponentGroupList[ActiveUIGroupIndex].RemoveGroupGraphics();
            ActiveUIGroupIndex--;

            UI_ComponentGroupList[ActiveUIGroupIndex].BackFromSelection(playerIndex);
            UI_ComponentGroupList[ActiveUIGroupIndex].MoveForward(backgroundSpacing);

        }
    }
}
