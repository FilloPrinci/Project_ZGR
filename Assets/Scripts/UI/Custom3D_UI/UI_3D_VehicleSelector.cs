using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.RayTracingAccelerationStructure;

public class UI_3D_VehicleSelector : MonoBehaviour
{
    public int playerIndex = 0;
    public float rotationSpeed = 5.0f;
    public float vehicleScale = 0.2f;
    public Transform vehicleSpawnPosition;
    public Transform pivotTransform;
    public Transform confirmTranform;
    public bool selectionCompleted = false;
    public GameObject HUD;
    public GameObject HUD_check;
    public GameObject SelectorLightMesh;
    [SerializeField, ColorUsage(true, true)]
    public Color NormalLightColor;
    [SerializeField, ColorUsage(true, true)]
    public Color SelectedLightColor;

    private RaceSettings _settings;
    private UI_3D_Manager _manager;
    private List<GameObject> availableVehicles;
    private GameObject currentSelectedVehicleInstance;

    private int currentSelectedVehicleIndex = 0;
    private GameObject mainCamera;

    private Renderer SelectorRenderer;
    private Material SelectorLightMaterial;

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
            availableVehicles = _settings.vehiclePrefabList;

            // spawn / show first veicchle
            currentSelectedVehicleInstance = Instantiate(availableVehicles[currentSelectedVehicleIndex], vehicleSpawnPosition);
            currentSelectedVehicleInstance.transform.localScale = Vector3.one * vehicleScale;

            if (HUD != null) {
                HUD.transform.position = vehicleSpawnPosition.position;
                HUD_check.SetActive(false);
            }

            pivotTransform.position = vehicleSpawnPosition.position;
            pivotTransform.rotation = Quaternion.identity;
        }

        _manager = UI_3D_Manager.Instance;

        if(_manager == null)
        {
            Debug.LogError("UI_3D_Manager instance not found!");
        }

        if (SelectorLightMesh != null) {
            SelectorRenderer = SelectorLightMesh.GetComponent<Renderer>();
            SelectorLightMaterial = SelectorRenderer.material;

            if (SelectorLightMaterial != null)
            {
                SelectorLightMaterial.SetColor("_Color", NormalLightColor);
            }
        }

    }

    private void Update()
    {
        deltaTime = Time.deltaTime;

        // rotate vehicle
        if (currentSelectedVehicleInstance != null)
        {
            pivotTransform.Rotate(Vector3.up, rotationSpeed * deltaTime);

            if (selectionCompleted)
            {
                currentSelectedVehicleInstance.transform.rotation = Utils.ExpDecay(currentSelectedVehicleInstance.transform.rotation, confirmTranform.rotation, 3f, deltaTime);
            }
            else
            {
                currentSelectedVehicleInstance.transform.rotation = pivotTransform.rotation;

                
            }

                
        }   
    }

    void RefreshVehicleInstance()
    {
        DestroyImmediate(currentSelectedVehicleInstance);
        currentSelectedVehicleInstance = Instantiate(availableVehicles[currentSelectedVehicleIndex], vehicleSpawnPosition);
        currentSelectedVehicleInstance.transform.localScale = Vector3.one * vehicleScale;
    }

    public void SelectRight()
    {
        if (!selectionCompleted)
        {
            if (currentSelectedVehicleIndex < availableVehicles.Count - 1)
            {
                currentSelectedVehicleIndex++;
            }
            else
            {
                currentSelectedVehicleIndex = 0;
            }

            RefreshVehicleInstance();
        }
        
    }


    public void SelectLeft()
    {
        if (!selectionCompleted)
        {
            if (currentSelectedVehicleIndex > 0)
            {
                currentSelectedVehicleIndex--;
            }
            else
            {
                currentSelectedVehicleIndex = availableVehicles.Count - 1;
            }

            RefreshVehicleInstance();
        }
    }

    public void ConfirmSelection()
    {
        if (!selectionCompleted)
        {
            selectionCompleted = true;

            _settings.OnVehicleSelect(playerIndex, currentSelectedVehicleIndex);
            HUD_check.SetActive(true);
            _manager.OnVehicleSelectionReady();

            if(SelectorLightMaterial != null)
            {
                SelectorLightMaterial.SetColor("_Color", SelectedLightColor);
            }
        }
    }

    public void CancelSelection()
    {
        if (selectionCompleted)
        {
            selectionCompleted = false;
            HUD_check.SetActive(false);

            if (SelectorLightMaterial != null)
            {
                SelectorLightMaterial.SetColor("_Color", NormalLightColor);
            }
        }
        else
        {
            _manager.ManageBackFromVehicleSelection(playerIndex);
        }
    }

}
