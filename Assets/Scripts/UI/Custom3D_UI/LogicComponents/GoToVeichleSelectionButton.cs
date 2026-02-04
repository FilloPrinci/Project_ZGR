using UnityEngine;

public class GoToVeichleSelectionButton : Button
{
    public int playersAmount = 1;

    override
    public void OnConfirmSelection()
    {
        Debug.Log("Navigate to Vehicle Selection");
        if (_manager != null)
        {
            _manager.StartVeichleSelection(playersAmount);
            
        }
    }

}
