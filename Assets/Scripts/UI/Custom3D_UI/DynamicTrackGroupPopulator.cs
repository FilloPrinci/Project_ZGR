using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UI_GroupComponent))]
public class DynamicTrackGroupPopulator : MonoBehaviour
{
    [Header("Prefabs (copia dagli altri UI_GroupComponent della scena)")]
    public GameObject panelPrefab;

    [Header("Layout")]
    public Vector3 imageOffset = new Vector3(0f, 0f, -0.2f);
    [Tooltip("Larghezza target dell'immagine in world units")]
    public float targetImageWorldSize = 1.5f;

    [Header("Navigation")]
    public UI_GroupComponent nextGroupComponent;

    private UI_GroupComponent _group;

    private void Awake()
    {
        _group = GetComponent<UI_GroupComponent>();
    }

    private void Start()
    {
        SceneReferences sceneRefs = SceneReferences.Instance;
        if (sceneRefs == null)
        {
            Debug.LogError("[DynamicTrackGroupPopulator] SceneReferences instance not found");
            return;
        }

        List<TrackSceneData> tracks = sceneRefs.trackSceneDataList;
        if (tracks == null || tracks.Count == 0)
        {
            Debug.LogError("[DynamicTrackGroupPopulator] trackSceneDataList is empty");
            return;
        }

        _group.UIComponentList = new List<UI_Component_3D>();

        try
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                TrackSceneData trackData = tracks[i];

                if (!IsSceneInBuildSettings(trackData.sceneName))
                {
                    Debug.LogError($"[DynamicTrackGroupPopulator] La scena '{trackData.sceneName}' non è in Build Settings!");
                }

                GameObject logicGO = new GameObject($"Track{i}_Button");
                logicGO.transform.SetParent(transform);

                TrackSelectionButton btn = logicGO.AddComponent<TrackSelectionButton>();
                btn.trackIndex = i;
                btn.nextGroupComponent = nextGroupComponent;

                bool hasImage = trackData.previewImage != null;

                UI_GraphicComponent graphic = new UI_GraphicComponent();
                graphic.Panel = panelPrefab;
                graphic.Icon = null;
                graphic.TextGUI = null;

                if (hasImage)
                {
                    Sprite sprite = trackData.previewImage;
                    graphic.previewSprite = sprite;
                    graphic.IconOffset = imageOffset;

                    float spriteWorldWidth = sprite.bounds.size.x;
                    graphic.IconSize = spriteWorldWidth > 0f ? targetImageWorldSize / spriteWorldWidth : 1f;
                }

                UI_Component_3D component = new UI_Component_3D();
                component.ComponentName = trackData.displayName;
                component.GraphicComponent = graphic;
                component.LogicComponent = btn;

                _group.UIComponentList.Add(component);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DynamicTrackGroupPopulator] Eccezione durante la popolazione: {e}");
        }

        Debug.Log($"[DynamicTrackGroupPopulator] Populated {_group.UIComponentList.Count}/{tracks.Count} tracks");
    }

    private static bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string nameFromPath = System.IO.Path.GetFileNameWithoutExtension(path);
            if (nameFromPath == sceneName)
                return true;
        }
        return false;
    }
}
