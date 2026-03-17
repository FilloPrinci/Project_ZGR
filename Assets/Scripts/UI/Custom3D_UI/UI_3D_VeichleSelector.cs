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
    private List<GameObject> availableVeichles;
    private GameObject currentSelectedVeichleInstance;

    private int currentSelectedVeichleIndex = 0;
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
            availableVeichles = _settings.veichlePrefabList;

            // spawn / show first veicchle
            currentSelectedVeichleInstance = Instantiate(availableVeichles[currentSelectedVeichleIndex], veichleSpawnPosition);
            currentSelectedVeichleInstance.transform.localScale = Vector3.one * veichleScale;

            if (HUD != null) {
                HUD.transform.position = veichleSpawnPosition.position;
                HUD_check.SetActive(false);
            }

            pivotTransform.position = veichleSpawnPosition.position;
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

        // rotate veichle
        if (currentSelectedVeichleInstance != null)
        {
            pivotTransform.Rotate(Vector3.up, rotationSpeed * deltaTime);

            if (selectionCompleted)
            {
                currentSelectedVeichleInstance.transform.rotation = Utils.ExpDecay(currentSelectedVeichleInstance.transform.rotation, confirmTranform.rotation, 3f, deltaTime);
            }
            else
            {
                currentSelectedVeichleInstance.transform.rotation = pivotTransform.rotation;

                
            }

                
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
            _manager.ManageBackFromVeichleSelection(playerIndex);
        }
    }

}
