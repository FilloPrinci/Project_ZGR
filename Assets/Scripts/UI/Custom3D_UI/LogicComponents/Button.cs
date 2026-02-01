using UnityEngine;

public class Button: UI_Logic_Component
{
    protected UI_3D_Manager _manager;


    override
    public void Init()
    {
        _manager = UI_3D_Manager.Instance;
        if (_manager == null) {
            Debug.LogWarning("No UI_3D_Manager Instance detected");
        }
    }

    override
    public void OnConfirmSelection()
    {
        Debug.Log("Navigate to " +  nextGroupComponent.GroupName);
        if (_manager != null && nextGroupComponent != null) {
            _manager.NavigateTo(nextGroupComponent);
        }
    }

    override
    public void OnSelection()
    {

    }

}
