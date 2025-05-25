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
    public GameObject readyPanel;

    [Header("Selectable UI Elements")]
    public GameObject firstMainMenuSelected;
    public GameObject firstOptionSelected;
    public GameObject firstPlayOptionSelected;
    public GameObject firstSingleplayerOptionSelected;
    public GameObject firstMultiplayerOptionSelected;
    public GameObject firstReadySelected;

    private GameObject currentlyActivePanel;
    private GameObject lastSelectedObject;

    private SceneReferences sceneReferences;

    private void Start()
    {
        currentlyActivePanel = mainMenuPanel;
        sceneReferences = SceneReferences.Instance;
    }

    void SwitchPanel(GameObject newPanel, GameObject newSelectedObject)
    {
        newPanel.SetActive(true);
        currentlyActivePanel.SetActive(false);
        currentlyActivePanel = newPanel;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(newSelectedObject);
    }

    public void StartRace()
    {
        SceneManager.LoadScene(sceneReferences.raceScene);
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

    public void ShowGetReadyPanel()
    {
        lastSelectedObject = EventSystem.current.currentSelectedGameObject;
        readyPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstReadySelected);
    }

    public void HideGetReadyPanel()
    {
        readyPanel.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(lastSelectedObject);
    }

    public void OnStart()
    {
        string raceTracSceneName = sceneReferences.raceScene;

        RaceSettings currentRaceSettings = RaceSettings.Instance;
        if (currentRaceSettings == null)
        {
            Debug.Log($"[MainMenuManager] : No RaceSettings Instance found, using editor settings");
        }
        else
        {
            Debug.Log($"[MainMenuManager] : RaceSettings Instance found, using player configured settings");

            raceTracSceneName = currentRaceSettings.GetSelectedRaceTrack();
        }
        SceneManager.LoadScene(raceTracSceneName);
    }
}
