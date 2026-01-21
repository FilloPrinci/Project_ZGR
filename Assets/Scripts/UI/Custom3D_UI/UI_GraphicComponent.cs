using UnityEngine;

[System.Serializable]
public class UI_GraphicComponent
{
    public GameObject Panel;
    public GameObject Icon;
    public float IconSize = 1.0f;
    public Vector3 IconOffset = new(0, 0, -0.2f);

    private GameObject instantiatedPanel;
    private GameObject instantiatedIcon;

    public GameObject GetInstantiatedPanel()
    {
        return instantiatedPanel;
    }

    public void SetInstantiatedPanel(GameObject panel)
    {
        instantiatedPanel = panel;
    }

    public GameObject GetInstantiatedIcon()
    {
        return instantiatedIcon;
    }

    public void SetInstantiatedIcon(GameObject icon)
    {
        instantiatedIcon = icon;
    }

}
