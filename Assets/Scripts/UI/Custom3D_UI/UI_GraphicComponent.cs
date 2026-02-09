using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class UI_GraphicComponent
{
    public GameObject Panel;
    public GameObject Icon;
    public GameObject TextGUI;
    public string text;
    public float IconSize = 1.0f;
    public Vector3 IconOffset = new(0, 0, -0.2f);
    public Vector3 TextOffset = new(-0.5f, 0, -0.2f);

    private GameObject instantiatedPanel;
    private GameObject instantiatedIcon;
    private GameObject instantiatedText;


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

    public GameObject GetInstantiatedText()
    {
        return instantiatedText;
    }

    public void SetInstantiatedText(GameObject text)
    {
        instantiatedText = text;
    }

    public string GetText() {

        if(instantiatedText != null)
        {
            TextMeshProUGUI textMeshProUGUI = instantiatedText.GetComponent<TextMeshProUGUI>();
            if (textMeshProUGUI != null) { 
                return textMeshProUGUI.text;
            }
            else
            {
                return "ERROR: no TextMeshProUGUI component found!";
            }
        }
        else
        {
            return "ERROR: no Text Gameobject assigned!";
        }
    }

    public void SetTextString(string text)
    {
        if (instantiatedText != null)
        {
            TextMeshProUGUI textMeshProUGUI = instantiatedText.GetComponent<TextMeshProUGUI>();
            if (textMeshProUGUI != null)
            {
                textMeshProUGUI.text = text;
            }
            else
            {
                Debug.LogError("Can't set string " + text + ", textMeshProUGUI component not found ");
            }
        }
        else
        {
            Debug.LogError("Can't set string " + text + ", instance is null ");
        }
    }

}
