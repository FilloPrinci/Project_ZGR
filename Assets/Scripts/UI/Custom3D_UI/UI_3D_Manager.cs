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

    public void SelectComponent()
    {

    }
}
