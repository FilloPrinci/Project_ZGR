using UnityEngine;

public class NavigationButton: UI_Logic_Component
{

    override
    public void OnConfirmSelection()
    {
        Debug.Log("Navigate to " +  nextGroupComponent.GroupName);
    }

    override
    public void OnSelection()
    {

    }

}
