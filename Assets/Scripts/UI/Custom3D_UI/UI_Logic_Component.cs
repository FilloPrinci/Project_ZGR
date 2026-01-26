using UnityEngine;

public abstract class UI_Logic_Component : MonoBehaviour
{
    public UI_GroupComponent nextGroupComponent;

    public abstract void Init();
    public abstract void OnConfirmSelection();
    public abstract void OnSelection();
}

