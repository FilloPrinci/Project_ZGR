using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.RayTracingAccelerationStructure;

public class UI_3D_VeichleSelector : MonoBehaviour
{
    public int playerIndex = 0;
    public Transform veichleSpawnPosition;
    public bool selectionCompleted = false;

    private RaceSettings _settings;
    private List<GameObject> availableVeichles;
    private GameObject currentSelectedVeichleInstance;
    private int currentSelectedVeichleIndex = 0;

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
        }
    }

    void RefreshVeichleInstance()
    {
        DestroyImmediate(currentSelectedVeichleInstance);
        currentSelectedVeichleInstance = Instantiate(availableVeichles[currentSelectedVeichleIndex], veichleSpawnPosition);
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
