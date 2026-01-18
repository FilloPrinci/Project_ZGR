using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class UI_GroupComponent : MonoBehaviour
{
    public List<UI_Component_3D> UIComponentList;
    public float GroupSpacing = 1.5f;
    public int ActiveComponentIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < UIComponentList.Count; i++)
        {
            UI_Component_3D component = UIComponentList[i];
            if (i == ActiveComponentIndex)
            {
                component.GraphicComponent.IconSize = 1;
            }
            else
            {
                component.GraphicComponent.IconSize = 0.5f;
            }
        }

        RefreshGraphics();
    }

    private void RefreshGraphics()
    {
        for (int i = 0; i < UIComponentList.Count; i++)
        {
            UI_Component_3D component = UIComponentList[i];
            UI_GraphicComponent graphicComponent = component.GraphicComponent;
            if (graphicComponent != null)
            {
                if (graphicComponent.GetInstantiatedPanel() != null)
                {
                    graphicComponent.GetInstantiatedPanel().transform.position = transform.position + new Vector3(i * GroupSpacing, 0, 0);
                }
                if (graphicComponent.GetInstantiatedIcon() != null)
                {
                    graphicComponent.GetInstantiatedIcon().transform.position = transform.position + new Vector3(i * GroupSpacing, 0, 0) + graphicComponent.IconOffset;
                }

                if(ActiveComponentIndex == i)
                {
                    graphicComponent.GetInstantiatedIcon().transform.localScale = Vector3.one * 1.0f;
                }
                else
                {
                    graphicComponent.GetInstantiatedIcon().transform.localScale = Vector3.one * 0.5f;
                }
            }
        }
    }

    public void InstantiateGroupGraphics()
    {
        if (UIComponentList != null && UIComponentList.Count > 0)
        {
            for (int i = 0; i < UIComponentList.Count; i++)
            {
                UI_Component_3D component = UIComponentList[i];
                UI_GraphicComponent graphicComponent = component.GraphicComponent;

                if (graphicComponent != null)
                {
                    Vector3 newInstancePosition = transform.position + new Vector3(i * GroupSpacing,0 , 0);
                    if (graphicComponent.Panel != null)
                    {
                        graphicComponent.SetInstantiatedPanel(Instantiate(graphicComponent.Panel, newInstancePosition, Quaternion.identity, transform));
                    }
                    if (graphicComponent.Icon != null)
                    {
                        graphicComponent.SetInstantiatedIcon(Instantiate(graphicComponent.Icon, newInstancePosition + graphicComponent.IconOffset, Quaternion.identity, transform));
                    }
                }
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
        ActiveComponentIndex = (ActiveComponentIndex + 1) % UIComponentList.Count;
        Debug.Log("Player " + playerIndex + " selected right to component index " + ActiveComponentIndex);
    }
}
