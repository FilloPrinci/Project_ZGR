using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SingleSelector : MonoBehaviour
{
    public int playerIndex = 0;
    public Transform shownVehiclePosition;
    
    public float scaleFactor = 0.2f;
    public GameObject selectionCheckIcon;
    private int selectedVehicleIndex = 0;
    private GameObject instantiatedSelectedVehicle;
    private RaceSettings raceSettings;
    private SelectionManager selectionManager;
    private bool selectionConfirmed = false;
    private List<GameObject> vehiclePrefabList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shownVehiclePosition.localScale = Vector3.one * scaleFactor; // Set the scale of the shown vehicle position

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
        vehiclePrefabList = raceSettings.vehiclePrefabList;
        if (instantiatedSelectedVehicle != null)
        {
            Destroy(instantiatedSelectedVehicle);
            
        }
        instantiatedSelectedVehicle = Instantiate(GetSelectedVehiclePrefab(), shownVehiclePosition);
    }

    void UpdateSelectedVehicle()
    {
        if (vehiclePrefabList.Count == 0)
        {
            Debug.LogError("[VehicleSelector] ERROR: vehiclePrefabList is empty");
            return;
        }

        if (instantiatedSelectedVehicle != null)
        {
            Destroy(instantiatedSelectedVehicle);
            instantiatedSelectedVehicle = Instantiate(GetSelectedVehiclePrefab(), shownVehiclePosition);
        }
    }

    public void ResetSelecton()
    {
        RefreshReferences();
        selectionConfirmed = false;
        selectionCheckIcon.SetActive(selectionConfirmed);
    }

    public void OnSelectNext(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!selectionConfirmed)
            {
                selectedVehicleIndex++;
                if (selectedVehicleIndex >= vehiclePrefabList.Count)
                {
                    selectedVehicleIndex = 0; // Loop back to the first vehicle
                }
                Debug.Log("[VehicleSelector] INFO: Player " + playerIndex + " selected next vehicle: " + selectedVehicleIndex);

                UpdateSelectedVehicle();
            }
        }
        
        
    }

    public void OnSelectPrev(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (!selectionConfirmed)
            {
                selectedVehicleIndex--;
                if (selectedVehicleIndex < 0)
                {
                    selectedVehicleIndex = vehiclePrefabList.Count-1; // Loop back to the last vehicle
                }
                Debug.Log("[VehicleSelector] INFO: Player " + playerIndex + " selected next vehicle: " + selectedVehicleIndex);

                UpdateSelectedVehicle();
            }
        }


    }

    public void OnKeyTest(int value)
    {
        Debug.Log("[VehicleSelector] INFO: Player " + playerIndex + " pressed test key with value: " + value);
    }

    public GameObject GetSelectedVehiclePrefab()
    {
        if (vehiclePrefabList.Count == 0)
        {
            Debug.LogError("[VehicleSelector] ERROR: vehiclePrefabList is empty");
            return null;
        }

        return vehiclePrefabList[selectedVehicleIndex];
    }

    public void OnSelectionConfirm()
    {
        if (isActiveAndEnabled)
        {
            selectionConfirmed = !selectionConfirmed;

            selectionCheckIcon.SetActive(selectionConfirmed);

            if (selectionConfirmed)
            {
                raceSettings.SetSelectedVehicleForPlayer(playerIndex, GetSelectedVehiclePrefab());
                selectionManager.OnVehicleSelected();
            }
            else
            {
                selectionManager.OnVehicleUnselect();
            }



            
        }
        
    }
}
