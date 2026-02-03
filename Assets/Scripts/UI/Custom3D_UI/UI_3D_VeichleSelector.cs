using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.RayTracingAccelerationStructure;

public class UI_3D_VeichleSelector : MonoBehaviour
{
    public int playerIndex = 0;
    public float rotationSpeed = 5.0f;
    public float veichleScale = 0.2f;
    public Transform veichleSpawnPosition;
    public bool selectionCompleted = false;
    public GameObject HUD;
    public GameObject HUD_check;

    private RaceSettings _settings;
    private UI_3D_Manager _manager;
    private List<GameObject> availableVeichles;
    private GameObject currentSelectedVeichleInstance;
    private int currentSelectedVeichleIndex = 0;
    private GameObject mainCamera;

    private float deltaTime;

    public void SetMainCamera(GameObject mainCamera)
    {
        this.mainCamera = mainCamera;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _settings = RaceSettings.Instance;
        

        if( _settings == null)
        {
            Debug.LogError("RaceSettigs instance not found!");
        }
        else
        {
            availableVeichles = _settings.veichlePrefabList;

            // spawn / show first veicchle
            currentSelectedVeichleInstance = Instantiate(availableVeichles[currentSelectedVeichleIndex], veichleSpawnPosition);
            currentSelectedVeichleInstance.transform.localScale = Vector3.one * veichleScale;

            if (HUD != null) {
                HUD.transform.position = veichleSpawnPosition.position;
                HUD_check.SetActive(false);
            }
        }

        _manager = UI_3D_Manager.Instance;

        if(_manager == null)
        {
            Debug.LogError("UI_3D_Manager instance not found!");
        }

    }

    private void Update()
    {
        deltaTime = Time.deltaTime;

        // rotate veichle
        if (currentSelectedVeichleInstance != null && !selectionCompleted)
        {
            currentSelectedVeichleInstance.transform.Rotate(Vector3.up, rotationSpeed * deltaTime);
        }

        // make HUD look at camera
        if (this.mainCamera != null) {
            HUD.transform.LookAt(this.mainCamera.transform);
        }
        
    }

    void RefreshVeichleInstance()
    {
        DestroyImmediate(currentSelectedVeichleInstance);
        currentSelectedVeichleInstance = Instantiate(availableVeichles[currentSelectedVeichleIndex], veichleSpawnPosition);
        currentSelectedVeichleInstance.transform.localScale = Vector3.one * veichleScale;
    }

    public void SelectRight()
    {
        if (!selectionCompleted)
        {
            if (currentSelectedVeichleIndex < availableVeichles.Count - 1)
            {
                currentSelectedVeichleIndex++;
            }
            else
            {
                currentSelectedVeichleIndex = 0;
            }

            RefreshVeichleInstance();
        }
        
    }


    public void SelectLeft()
    {
        if (!selectionCompleted)
        {
            if (currentSelectedVeichleIndex > 0)
            {
                currentSelectedVeichleIndex--;
            }
            else
            {
                currentSelectedVeichleIndex = availableVeichles.Count - 1;
            }

            RefreshVeichleInstance();
        }
    }

    public void ConfirmSelection()
    {
        if (!selectionCompleted)
        {
            selectionCompleted = true;

            _settings.OnVeichleSelect(playerIndex, currentSelectedVeichleIndex);
            HUD_check.SetActive(true);
            _manager.OnVeichleSelectionReady();
        }
    }

    public void CancelSelection()
    {
        if (selectionCompleted)
        {
            selectionCompleted = false;
            HUD_check.SetActive(false);
        }
        else
        {
            _manager.ManageBackFromVeichleSelection(playerIndex);
        }
    }

}
