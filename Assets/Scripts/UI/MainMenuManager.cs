using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEditor.SearchService;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject optionsPanel;
    public GameObject mainMenuPanel;

    [Header("Selectable UI Elements")]
    public GameObject firstOptionSelected;
    public GameObject firstMainMenuSelected;

    public string playScene;

    public void OnPlayPressed()
    {
        SceneManager.LoadScene(playScene);
    }

    public void OnOptionsPressed()
    {
        optionsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);

        // Imposta la selezione sul primo bottone delle opzioni
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstOptionSelected);
    }

    public void OnQuitPressed()
    {
        Application.Quit();
        Debug.Log("Quit pressed");
    }

    public void OnBackFromOptions()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        // Imposta la selezione sul primo bottone del menu principale
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstMainMenuSelected);
    }
}
