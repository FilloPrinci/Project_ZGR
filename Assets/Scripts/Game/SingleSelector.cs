using System.Collections.Generic;
using UnityEngine;

public class SingleSelector : MonoBehaviour
{
    public int playerIndex = 0;
    public Transform shownVeichlePosition;
    
    public float scaleFactor = 0.2f;
    private int selectedVeichleIndex = 0;
    private GameObject instantiatedSelectedVeichle;
    private RaceSettings raceSettings;
    private SelectionManager selectionManager;
    private bool selectionConfirmed = false;
    private List<GameObject> veichlePrefabList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shownVeichlePosition.localScale = Vector3.one * scaleFactor; // Set the scale of the shown veichle position
        
        raceSettings = RaceSettings.Instance;
        selectionManager = SelectionManager.Instance;
        veichlePrefabList = raceSettings.veichlePrefabList;
        instantiatedSelectedVeichle = Instantiate(GetSelectedVeichlePrefab(), shownVeichlePosition);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateSelectedVeichle()
    {
        if (veichlePrefabList.Count == 0)
        {
            Debug.LogError("[VeichleSelector] ERROR: veichlePrefabList is empty");
            return;
        }

        if (instantiatedSelectedVeichle != null)
        {
            Destroy(instantiatedSelectedVeichle);
            instantiatedSelectedVeichle = Instantiate(GetSelectedVeichlePrefab(), shownVeichlePosition);
        }
    }

    public void SelectNext()
    {
        if (!selectionConfirmed) {
            selectedVeichleIndex++;
            if (selectedVeichleIndex >= veichlePrefabList.Count)
            {
                selectedVeichleIndex = 0; // Loop back to the first veichle
            }
            UpdateSelectedVeichle();
        }
        
    }

    public GameObject GetSelectedVeichlePrefab()
    {
        if (veichlePrefabList.Count == 0)
        {
            Debug.LogError("[VeichleSelector] ERROR: veichlePrefabList is empty");
            return null;
        }

        return veichlePrefabList[selectedVeichleIndex];
    }

    public void OnSelectionConfirm()
    {
        if (isActiveAndEnabled)
        {
            if (!selectionConfirmed)
            {
                selectionConfirmed = true;
                raceSettings.SetSelectedVeichleForPlayer(playerIndex, GetSelectedVeichlePrefab());
                selectionManager.OnVeichleSelected();
            }
            else
            {
                selectionConfirmed = false;
                selectionManager.OnVeichleUnselect();
            }
        }
        
    }
}
