using UnityEngine;

public class GoToVehicleSelectionButton : Button
{
    public int playersAmount = 1;

    override
    public void OnConfirmSelection()
    {
        Debug.Log("Navigate to Vehicle Selection");
        if (_manager != null)
        {
            _manager.StartVehicleSelection(playersAmount);
            
        }
    }

}
