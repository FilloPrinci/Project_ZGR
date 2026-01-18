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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (UI_ComponentGroupList != null)
        {
            UI_ComponentGroupList[ActiveUIGroupIndex].InstantiateGroupGraphics();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void ManageSelectRight(int playerIndex)
    {
        UI_ComponentGroupList[ActiveUIGroupIndex].SelectRight(playerIndex);
    }

    public void SelectComponent()
    {

    }
}
