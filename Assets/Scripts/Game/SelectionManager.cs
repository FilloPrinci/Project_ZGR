using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum SelectionType
{
    Veichle,
    Track,
    Mode
}

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; }
    public List<GameObject> selectorList;
    public Transform cameraPivot;
    public GameObject sceneCamera;
    public float cameraMinDistance = 5f;
    public float cameraMaxDistance = 8f;
    public float cameraHeight = 5f;
    public float cameraXRotation = 10f;

    private RaceSettings currentRaceSettings;
    private SceneReferences sceneReferences;
    private int veichleSelectedAmount = 0;

    private SelectionType[] selectionPhases = new SelectionType[]
    {
        SelectionType.Veichle,
        SelectionType.Track,
        SelectionType.Mode
    };

    private SelectionType currentSelectionPhase;
    private int currentSelectionPhaseIndex = 0;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogError("Duplicate RaceSettings detected. Destroying extra instance.");
            Destroy(gameObject); // Assicura che ci sia solo un RaceSettings
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentRaceSettings = RaceSettings.Instance;
        sceneReferences = SceneReferences.Instance;
        currentSelectionPhase = selectionPhases[currentSelectionPhaseIndex];

        foreach(GameObject selector in selectorList)
        {
            if (selector != null)
            {
                selector.SetActive(false);
            }
            else
            {
                Debug.LogError("[SelectionManager] ERROR: One of the selectors in selectorList is null");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void MoveToNextSelectionPhase()
    {
        Debug.Log("[SelectionManager] INFO: Moving to next selection phase");
        currentSelectionPhaseIndex++;
        currentSelectionPhase = selectionPhases[currentSelectionPhaseIndex];
        StartSelection();
    }

    private Transform CalculateCameraPivotPosition()
    {
        if (selectorList.Count == 0)
        {
            Debug.LogError("[SelectionManager] ERROR: selectorList is empty");
            return null;
        }
        // Calculate the average position of all active selectors
        Vector3 averagePosition = Vector3.zero;

        int activeCount = 0;
        foreach (GameObject selector in selectorList)
        {
            if(selector.activeSelf)
            {
                averagePosition += selector.transform.position;
                activeCount++;
            }
            
        }
        averagePosition /= activeCount;
        // Create a new Transform to represent the camera pivot
        Transform pivot = new GameObject("CameraPivot").transform;
        pivot.position = averagePosition;
        float distance = cameraMinDistance + (cameraMaxDistance - cameraMinDistance) / selectorList.Count * activeCount;
        pivot.position = new Vector3(pivot.position.x, cameraHeight, -distance);

        return pivot;
    }

    private void UpdateCameraPosition()
    {
        cameraPivot.position = CalculateCameraPivotPosition().position;

        if (cameraPivot != null)
        {
            sceneCamera.transform.position = cameraPivot.position;
            sceneCamera.transform.rotation = Quaternion.Euler(cameraXRotation, 0, 0);
        }
        else
        {
            Debug.LogError("[SelectionManager] ERROR: cameraPivot is not assigned");
            return;
        }
    }

    private void StartVeichleSelection() {
        Debug.Log("[SelectionManager] INFO: Starting veichle selection phase");

        if (selectorList.Count == 0)
        {
            Debug.LogError("[SelectionManager] ERROR: selectorList is empty");
            return;
        }
        else
        {
            for (int i = 0; i < currentRaceSettings.inputPlayersAmount; i++)
            {
                if (i < selectorList.Count)
                {
                    selectorList[i].SetActive(true);
                }
                else
                {
                    Debug.LogWarning("[SelectionManager] Not enough selectors for the number of players. Some players will not be able to select.");
                }
            }

            UpdateCameraPosition();
        }
    }

    private void StartTrackSelection()
    {
        Debug.Log("[SelectionManager] INFO: Starting track selection phase");
        foreach(GameObject selector in selectorList)
        {
            if (selector != null)
            {
                selector.SetActive(false); // Hide veichle selectors
            }
            else
            {
                Debug.LogError("[SelectionManager] ERROR: One of the selectors in selectorList is null");
            }
        }

        selectorList[0].SetActive(true); // Activate the track selector

        UpdateCameraPosition();
    }

    public void StartSelection()
    {
        if (currentSelectionPhase == SelectionType.Veichle)
        {
            StartVeichleSelection();
        }
        else if (currentSelectionPhase == SelectionType.Track)
        {
            SceneManager.LoadScene(sceneReferences.raceTrackSceneList[currentRaceSettings.defaultRaceTrackIndex]);
            //StartTrackSelection();
        }

        
    }

    public void OnVeichleSelected()
    {
        veichleSelectedAmount++;
        if (veichleSelectedAmount >= currentRaceSettings.inputPlayersAmount)
        {
            // All veichles selected, move to the next selection phase
            MoveToNextSelectionPhase();
        }
    }

    public void OnVeichleUnselect()
    {
        veichleSelectedAmount--;
        if (veichleSelectedAmount < 0)
        {
            veichleSelectedAmount = 0; // Prevent negative count
        }
    }

}
