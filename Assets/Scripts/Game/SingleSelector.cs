using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

        RefreshReferences();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void RefreshReferences()
    {
        raceSettings = RaceSettings.Instance;
        selectionManager = SelectionManager.Instance;
        veichlePrefabList = raceSettings.veichlePrefabList;
        if (instantiatedSelectedVeichle != null)
        {
            Destroy(instantiatedSelectedVeichle);
            
        }
        instantiatedSelectedVeichle = Instantiate(GetSelectedVeichlePrefab(), shownVeichlePosition);
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

    public void ResetSelecton()
    {
        RefreshReferences();
        selectionConfirmed = false;
    }

    public void OnSelectNext(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!selectionConfirmed)
            {
                selectedVeichleIndex++;
                if (selectedVeichleIndex >= veichlePrefabList.Count)
                {
                    selectedVeichleIndex = 0; // Loop back to the first veichle
                }
                Debug.Log("[VeichleSelector] INFO: Player " + playerIndex + " selected next veichle: " + selectedVeichleIndex);

                UpdateSelectedVeichle();
            }
        }
        
        
    }

    public void OnKeyTest(int value)
    {
        Debug.Log("[VeichleSelector] INFO: Player " + playerIndex + " pressed test key with value: " + value);
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
