using UnityEngine;

[System.Serializable]
public class UI_Component_3D
{
    public string ComponentName;
    public UI_GraphicComponent GraphicComponent;
    public UI_Logic_Component LogicComponent;
    public bool IsActive = false;
    public UI_GroupComponent selectorComponent;

    public bool IsSelector()
    {
        bool isSelector = (selectorComponent != null) && ((selectorComponent.groupScrollAxis == GroupScrollAxis.Vertical)) && (selectorComponent.UIComponentList != null) && (selectorComponent.UIComponentList.Count > 0); 

        return isSelector;
    }
}
