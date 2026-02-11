using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum GroupScrollAxis
{
    Horizontal,
    Vertical
}

public class UI_GroupComponent : MonoBehaviour
{
    public string GroupName;
    public GroupScrollAxis groupScrollAxis = GroupScrollAxis.Horizontal;

    public List<UI_Component_3D> UIComponentList;

    public int ActiveComponentIndex = 0;

    private bool selectionConfirmed = false;
    public bool inSubMenu = false;

    private UI_GroupComponent activeGroupSelector;

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
                graphicComponent.GetInstantiatedIcon().transform.localPosition = currentPositionList[i] + graphicComponent.IconOffset;

                if(graphicComponent.GetInstantiatedText() != null)
                {
                    graphicComponent.GetInstantiatedText().transform.localPosition = currentPositionList[i] + graphicComponent.TextOffset;
                }

                //refresh panel and icon size
                currentPanelSizeList[i] = Utils.ExpDecay(currentPanelSizeList[i], newPanelSizeList[i], panelScaleSpeed, deltaTime);
                currentIconSizeList[i] = Utils.ExpDecay(currentIconSizeList[i], newIconSizeList[i], iconScaleSpeed, deltaTime);

                if (ActiveComponentIndex == i)
                {
                    newIconSizeList[i] = graphicComponent.IconSize * iconScaleMultiplier;
                    newPanelSizeList[i] = panelScaleMultiplier;
                }
                else
                {
                    newIconSizeList[i] = graphicComponent.IconSize;
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
                    if(graphicComponent.GetInstantiatedText() != null)
                    {
                        graphicComponent.GetInstantiatedText().SetActive(false);
                    }
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

    public void InstantiateGroupGraphics(Vector3? newOriginPosition = null)
    {
        Vector3 originPosition;

        if(newOriginPosition != null)
        {
            originPosition = (Vector3)newOriginPosition;
        }
        else
        {
            originPosition = Vector3.zero;
        }


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

                Vector3 newInstancePosition;

                if (groupScrollAxis == GroupScrollAxis.Horizontal)
                {
                    newInstancePosition = originPosition + (Vector3.right * i * groupSpacing);
                }
                else
                {
                    newInstancePosition = originPosition + (Vector3.down * i * groupSpacing);
                }


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
                    if(graphicComponent.TextGUI != null)
                    {
                        graphicComponent.SetInstantiatedText(Instantiate(graphicComponent.TextGUI, newInstancePosition + graphicComponent.TextOffset, transform.rotation, transform));
                        graphicComponent.SetTextString(graphicComponent.text);
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

    public void InitializeGroupLogic()
    {
        for (int i = 0; i < UIComponentList.Count; i++)
        {
            UI_Component_3D component = UIComponentList[i];
            if (component.LogicComponent != null)
            {
                component.LogicComponent.Init();
            }
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
                if (graphicComponent.GetInstantiatedText() != null)
                {
                    Destroy(graphicComponent.GetInstantiatedText());
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
        if (groupScrollAxis == GroupScrollAxis.Vertical)
        {
            Debug.LogWarning("Trying to select right on a vertical group component, this is not allowed");
            return;
        }

        if (ActiveComponentIndex == positionList.Count - 1)
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

        SelectComponent();
    }

    public void SelectLeft(int playerIndex)
    {
        if(groupScrollAxis == GroupScrollAxis.Vertical)
        {
            Debug.LogWarning("Trying to select left on a vertical group component, this is not allowed");
            return;
        }

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

        SelectComponent();
    }

    public void SelectUp(int playerIndex)
    {
        if(activeGroupSelector != null)
        {
            activeGroupSelector.SelectUp(playerIndex);
        }
        else
        {
            if (groupScrollAxis == GroupScrollAxis.Horizontal)
            {
                Debug.LogWarning("Trying to select up on a horizontal group component, this is not allowed");
                return;
            }
            Debug.Log("Select up");
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
                    positionList[i] += Vector3.up * -groupSpacing;
                }
            }


        }
    }

    public void SelectDown(int playerIndex)
    {
        if (activeGroupSelector != null)
        {
            activeGroupSelector.SelectDown(playerIndex);
        }
        else
        {
            if (groupScrollAxis == GroupScrollAxis.Horizontal)
            {
                Debug.LogWarning("Trying to select down on a horizontal group component, this is not allowed");
                return;
            }
            Debug.Log("Select down");
            if (ActiveComponentIndex == positionList.Count - 1)
            {
                //don't wrap around

                return;
            }
            else
            {
                ActiveComponentIndex++;
                for (int i = 0; i < positionList.Count; i++)
                {
                    positionList[i] += Vector3.up * groupSpacing;
                }
            }
        }
    }

    public void SelectComponent()
    {
        UI_Logic_Component logicComponent = UIComponentList[ActiveComponentIndex].LogicComponent;
        if (logicComponent != null)
        {
            logicComponent.OnSelection();
        }
    }

    public void ConfirmSelection(int playerIndex)
    {
        if (!selectionConfirmed)
        {
            selectionConfirmed = true;
            HideUnselectedComponents();

            UI_Component_3D selected_UI_Component = UIComponentList[ActiveComponentIndex];

            if(selected_UI_Component.IsSelector() && !inSubMenu)
            {
                // show selector group
                selected_UI_Component.selectorComponent.Setup(
                    this.groupSpacing / 2,
                    this.moveSpeed,
                    this.panelScaleMultiplier,
                    this.panelScaleSpeed,
                    this.iconScaleMultiplier,
                    this.iconScaleSpeed
                    );
                selected_UI_Component.selectorComponent.InstantiateGroupGraphics(position + (Vector3.right * groupSpacing));
                activeGroupSelector = selected_UI_Component.selectorComponent;
                selected_UI_Component.selectorComponent.ResetSelection();
                inSubMenu = true;
            }
            else 
            {
                // execute logic

                Debug.Log("Executing logic for" + GroupName);

                if(groupScrollAxis != GroupScrollAxis.Vertical)
                {
                    UI_Logic_Component logicComponent = UIComponentList[ActiveComponentIndex].LogicComponent;
                    if (logicComponent != null)
                    {
                        logicComponent.OnConfirmSelection();
                    }
                }

                selected_UI_Component.ExecuteAction();
            }
        }
        else
        {
            UI_Component_3D selected_UI_Component = UIComponentList[ActiveComponentIndex];

            if (selected_UI_Component.IsSelector() && inSubMenu)
            {
                // confirm selection in active subgroup
                selected_UI_Component.selectorComponent.ConfirmSelection(playerIndex);
            }
            else
            {
                Debug.LogWarning("Selection is already confirmed");
            }
        }

    }

    public void BackFromSubMenu(int playerIndex)
    {
        Debug.Log("BackFromSubMenu for" + GroupName);

        if (selectionConfirmed)
        {
            UI_Component_3D selected_UI_Component = UIComponentList[ActiveComponentIndex];

            if (selected_UI_Component.IsSelector() && inSubMenu)
            {
                // back from active subgroup
                selected_UI_Component.selectorComponent.RemoveGroupGraphics();
                activeGroupSelector = null;
                inSubMenu = false;
                selectionConfirmed = false;
            }
        }
    }

    public void BackFromSelection(int playerIndex)
    {
        Debug.Log("BackFromSelection for" + GroupName);

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

    public string GetCurrentSelectionName()
    {
        return UIComponentList[ActiveComponentIndex].ComponentName;
    }

    private void ResetSelection()
    {
        selectionConfirmed = false;
        ActiveComponentIndex = 0;
    }
}
