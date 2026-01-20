using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UI_GroupComponent : MonoBehaviour
{
    public List<UI_Component_3D> UIComponentList;
    
    public int ActiveComponentIndex = 0;
    
    private List<Vector3> positionList;
    private List<Vector3> newPositionList;
    private List<Vector3> currentPositionList;

    private List<float> currentPanelSizeList;
    private List<float> newPanelSizeList;

    private List<float> currentIconSizeList;
    private List<float> newIconSizeList;

    private float groupSpacing;
    private float moveSpeed;
    private float panelScaleMultiplier;
    private float panelScaleSpeed;
    private float iconScaleMultiplier;
    private float iconScaleSpeed;

    private float deltaTime;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        deltaTime = Time.deltaTime;
        RefreshGraphics();
    }

    public void Setup(
        float groupSpacing,
        float moveSpeed,
        float panelScaleMultiplier,
        float panelScaleSpeed,
        float iconScaleMultiplier,
        float iconScaleSpeed
        )
    {
        this.groupSpacing = groupSpacing;
        this.moveSpeed = moveSpeed;
        this.panelScaleMultiplier = panelScaleMultiplier;
        this.panelScaleSpeed = panelScaleSpeed;
        this.iconScaleMultiplier = iconScaleMultiplier;
        this.iconScaleSpeed = iconScaleSpeed;
    }

    private void RefreshGraphics()
    {

        for (int i = 0; i < UIComponentList.Count; i++)
        {
            UI_Component_3D component = UIComponentList[i];
            UI_GraphicComponent graphicComponent = component.GraphicComponent;
            if (graphicComponent != null)
            {
                newPositionList[i] = positionList[i];


                currentPositionList[i] = graphicComponent.GetInstantiatedPanel().transform.position;
                currentPositionList[i] = Utils.ExpDecay(currentPositionList[i], newPositionList[i], moveSpeed, deltaTime);

                // refresh positions
                graphicComponent.GetInstantiatedPanel().transform.position = currentPositionList[i];
                graphicComponent.GetInstantiatedIcon().transform.position = currentPositionList[i] + graphicComponent.IconOffset;

                //refresh panel and icon size
                currentPanelSizeList[i] = Utils.ExpDecay(currentPanelSizeList[i], newPanelSizeList[i], panelScaleSpeed, deltaTime);
                currentIconSizeList[i] = Utils.ExpDecay(currentIconSizeList[i], newIconSizeList[i], iconScaleSpeed, deltaTime);

                if (ActiveComponentIndex == i)
                {
                    newIconSizeList[i] = iconScaleMultiplier;
                    newPanelSizeList[i] = panelScaleMultiplier;
                }
                else
                {
                    newIconSizeList[i] = 1f;
                    newPanelSizeList[i] = 1f;
                }

                graphicComponent.GetInstantiatedPanel().transform.localScale = Vector3.one * currentPanelSizeList[i];
                graphicComponent.GetInstantiatedIcon().transform.localScale = Vector3.one * currentIconSizeList[i];
            }
        }

    }

    public void InstantiateGroupGraphics()
    {
        if (UIComponentList != null && UIComponentList.Count > 0)
        {
            positionList = new List<Vector3>();
            newPositionList = new List<Vector3>();
            currentPositionList = new List<Vector3>();
            currentIconSizeList = new List<float>();
            newIconSizeList = new List<float>();
            currentPanelSizeList = new List<float>();
            newPanelSizeList = new List<float>();

            for (int i = 0; i < UIComponentList.Count; i++)
            {
                UI_Component_3D component = UIComponentList[i];
                UI_GraphicComponent graphicComponent = component.GraphicComponent;

                Vector3 newInstancePosition = transform.right * i * groupSpacing;

                if (graphicComponent != null)
                {
                    
                    if (graphicComponent.Panel != null)
                    {
                        graphicComponent.SetInstantiatedPanel(Instantiate(graphicComponent.Panel, newInstancePosition, transform.rotation, transform));
                    }
                    if (graphicComponent.Icon != null)
                    {
                        graphicComponent.SetInstantiatedIcon(Instantiate(graphicComponent.Icon, newInstancePosition + graphicComponent.IconOffset, transform.rotation, transform));
                    }
                }

                positionList.Add(newInstancePosition);
                newPositionList.Add(newInstancePosition);
                currentPositionList.Add(newInstancePosition);
                currentIconSizeList.Add(1f);
                newIconSizeList.Add(1f);
                currentPanelSizeList.Add(1f);
                newPanelSizeList.Add(1f);
            }
        }
        else
        {
            Debug.LogError("No UI Components assigned to the UI Group Component.");
            return;
        }
        
    }

    public void SelectRight(int playerIndex)
    {
        
        
        if(ActiveComponentIndex == positionList.Count - 1)
        {
            //don't wrap around
            
            return;
        }
        else
        {
            ActiveComponentIndex++;
            for (int i = 0; i < positionList.Count; i++)
            {
                positionList[i] +=  transform.right * -groupSpacing;
            }
        }

        Debug.Log("Player " + playerIndex + " selected right to component index " + ActiveComponentIndex);
    }

    public void SelectLeft(int playerIndex)
    {


        if (ActiveComponentIndex == 0)
        {
            //don't wrap around
            
            return;
        }
        else
        {
            ActiveComponentIndex--;
            for (int i = 0; i < positionList.Count; i++)
            {
                positionList[i] += transform.right * groupSpacing;
            }
        }

        Debug.Log("Player " + playerIndex + " selected left to component index " + ActiveComponentIndex);
    }
}
