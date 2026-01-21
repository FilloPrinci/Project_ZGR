using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UI_GroupComponent : MonoBehaviour
{
    public List<UI_Component_3D> UIComponentList;
    
    public int ActiveComponentIndex = 0;

    private bool selectionConfirmed = false;

    private Vector3 position;
    private Vector3 newPosition;
    private Vector3 currentPosition;

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
        position = transform.position;
        newPosition = transform.position;
        currentPosition = transform.position;
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
        if(positionList == null || positionList.Count == 0)
        {
            return;
        }

        for (int i = 0; i < UIComponentList.Count; i++)
        {
            UI_Component_3D component = UIComponentList[i];
            UI_GraphicComponent graphicComponent = component.GraphicComponent;
            if (graphicComponent != null)
            {
                newPositionList[i] = positionList[i];


                currentPositionList[i] = graphicComponent.GetInstantiatedPanel().transform.localPosition;
                currentPositionList[i] = Utils.ExpDecay(currentPositionList[i], newPositionList[i], moveSpeed, deltaTime);

                // refresh positions
                graphicComponent.GetInstantiatedPanel().transform.localPosition = currentPositionList[i];
                graphicComponent.GetInstantiatedIcon().transform.localPosition = currentPositionList[i] + Vector3.forward * graphicComponent.IconOffset.z;

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

        newPosition = position;
        currentPosition = transform.position;
        currentPosition = Utils.ExpDecay(currentPosition, newPosition, moveSpeed, deltaTime);
        transform.position = currentPosition;
        
    }

    

    public void HideUnselectedComponents()
    {
        if (selectionConfirmed)
        {
            // hide unselected components
            for (int i = 0; i < UIComponentList.Count; i++)
            {
                if (i != ActiveComponentIndex)
                {
                    UI_Component_3D component = UIComponentList[i];
                    UI_GraphicComponent graphicComponent = component.GraphicComponent;

                    graphicComponent.GetInstantiatedPanel().SetActive(false);
                    graphicComponent.GetInstantiatedIcon().SetActive(false);
                }
            }
        }
        else
        {
            Debug.LogWarning("Tryng to hide unselected componets but selection is not confirmed");
        }
    }

    public void ShowUnselectedComponents()
    {
        if (!selectionConfirmed)
        {
            // show unselected components
            for (int i = 0; i < UIComponentList.Count; i++)
            {
                if (i != ActiveComponentIndex)
                {
                    UI_Component_3D component = UIComponentList[i];
                    UI_GraphicComponent graphicComponent = component.GraphicComponent;

                    graphicComponent.GetInstantiatedPanel().SetActive(true);
                    graphicComponent.GetInstantiatedIcon().SetActive(true);
                }
            }
        }
        else
        {
            Debug.LogWarning("Tryng to show unselected componets but selection is confirmed");
        }
    }

    public void MoveBack(float amount)
    {
        position += transform.forward * amount;

        /*
        for (int i = 0; i < positionList.Count; i++)
        {
            positionList[i] += transform.forward * amount;
        }
        */
    }

    public void MoveForward(float amount)
    {
        position -= transform.forward * amount;

        /*
        for (int i = 0; i < positionList.Count; i++)
        {
            positionList[i] -= transform.forward * amount;
        }
        */
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

                Vector3 newInstancePosition = Vector3.right * i * groupSpacing;

                if (graphicComponent != null)
                {
                    
                    if (graphicComponent.Panel != null)
                    {
                        graphicComponent.SetInstantiatedPanel(Instantiate(graphicComponent.Panel, newInstancePosition, transform.rotation, transform));
                    }
                    if (graphicComponent.Icon != null)
                    {
                        graphicComponent.SetInstantiatedIcon(Instantiate(graphicComponent.Icon, newInstancePosition + Vector3.forward * graphicComponent.IconOffset.z, transform.rotation, transform));
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

    public void RemoveGroupGraphics()
    {
        for (int i = 0; i < UIComponentList.Count; i++)
        {
            UI_Component_3D component = UIComponentList[i];
            UI_GraphicComponent graphicComponent = component.GraphicComponent;
            if (graphicComponent != null)
            {
                if (graphicComponent.GetInstantiatedPanel() != null)
                {
                    Destroy(graphicComponent.GetInstantiatedPanel());
                }
                if (graphicComponent.GetInstantiatedIcon() != null)
                {
                    Destroy(graphicComponent.GetInstantiatedIcon());
                }
            }
        }
        positionList.Clear();
        newPositionList.Clear();
        currentPositionList.Clear();
        currentIconSizeList.Clear();
        newIconSizeList.Clear();
        currentPanelSizeList.Clear();
        newPanelSizeList.Clear();
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
                positionList[i] += Vector3.right * -groupSpacing;
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
                positionList[i] += Vector3.right * groupSpacing;
            }
        }

        Debug.Log("Player " + playerIndex + " selected left to component index " + ActiveComponentIndex);
    }

    public void ConfirmSelection(int playerIndex)
    {
        if (!selectionConfirmed)
        {
            selectionConfirmed = true;
            HideUnselectedComponents();
        }
        else
        {
            Debug.LogWarning("Selection is already confirmed");
        }

    }

    public void BackFromSelection(int playerIndex)
    {
        if (selectionConfirmed)
        {
            selectionConfirmed = false;
            ShowUnselectedComponents();
        }
        else
        {
            Debug.LogWarning("Selection is not yet confirmed");
        }
    }

}
