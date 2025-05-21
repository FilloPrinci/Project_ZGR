using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject optionsPanel;
    public GameObject mainMenuPanel;
    public GameObject playOptionsPanel;
    public GameObject singleplayerOptionsPanel;
    public GameObject multiplayerOptionsPanel;

    [Header("Selectable UI Elements")]
    public GameObject firstMainMenuSelected;
    public GameObject firstOptionSelected;
    public GameObject firstPlayOptionSelected;
    public GameObject firstSingleplayerOptionSelected;
    public GameObject firstMultiplayerOptionSelected;
    

    public string playScene;

    private GameObject currentlyActivePanel;

    private void Start()
    {
        currentlyActivePanel = mainMenuPanel;
    }

    void SwitchPanel(GameObject newPanel, GameObject newSelectedObject)
    {
        newPanel.SetActive(true);
        currentlyActivePanel.SetActive(false);
        currentlyActivePanel = newPanel;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(newSelectedObject);
    }

    public void OnPlayPressed()
    {
        SwitchPanel(playOptionsPanel, firstPlayOptionSelected);
    }

    public void OnSingleplayerPressed()
    {
        SwitchPanel(singleplayerOptionsPanel, firstSingleplayerOptionSelected);
    }

    public void OnMultiplayerPressed()
    {
        SwitchPanel(multiplayerOptionsPanel, firstMultiplayerOptionSelected);
    }

    public void OnOptionsPressed()
    {
        SwitchPanel(optionsPanel, firstOptionSelected);
    }

    public void OnQuitPressed()
    {
        Application.Quit();
        Debug.Log("Quit pressed");
    }

    public void OnBackToMainMenu()
    {
        SwitchPanel(mainMenuPanel, firstMainMenuSelected);
    }

    public void OnBackFromOptions()
    {
        SwitchPanel(mainMenuPanel, firstMainMenuSelected);
    }

    public void OnBackFromPlayOptions()
    {
        SwitchPanel(mainMenuPanel, firstMainMenuSelected);
    }
}
