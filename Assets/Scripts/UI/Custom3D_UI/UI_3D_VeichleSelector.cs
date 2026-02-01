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

    private RaceSettings _settings;
    private List<GameObject> availableVeichles;
    private GameObject currentSelectedVeichleInstance;
    private int currentSelectedVeichleIndex = 0;

    private float deltaTime;

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
    }

    void RefreshVeichleInstance()
    {
        DestroyImmediate(currentSelectedVeichleInstance);
        currentSelectedVeichleInstance = Instantiate(availableVeichles[currentSelectedVeichleIndex], veichleSpawnPosition);
        currentSelectedVeichleInstance.transform.localScale = Vector3.one * veichleScale;
    }

    public void SelectRight()
    {
        if(currentSelectedVeichleIndex < availableVeichles.Count - 1)
        {
            currentSelectedVeichleIndex++;
        }
        else
        {
            currentSelectedVeichleIndex = 0;
        }

        RefreshVeichleInstance();
    }


    public void SelectLeft()
    {
        if (currentSelectedVeichleIndex > 0)
        {
            currentSelectedVeichleIndex--;
        }
        else { 
            currentSelectedVeichleIndex = availableVeichles.Count - 1;
        }

        RefreshVeichleInstance();
    }

    public void ConfirmSelection()
    {
        if (!selectionCompleted)
        {
            selectionCompleted = true;

            _settings.SetSelectedVeichleForPlayer(playerIndex, availableVeichles[currentSelectedVeichleIndex]);
        }
    }

    public void CancelSelection()
    {
        if (selectionCompleted)
        {
            selectionCompleted = false;
        }
        else
        {
            // TODO: get back to previous menu
        }
    }

}
