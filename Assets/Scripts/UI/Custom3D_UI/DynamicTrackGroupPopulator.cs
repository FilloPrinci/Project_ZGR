using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UI_GroupComponent))]
public class DynamicTrackGroupPopulator : MonoBehaviour
{
    [Header("Prefabs (copia dagli altri UI_GroupComponent della scena)")]
    public GameObject panelPrefab;
    public GameObject textPrefab;
    [Tooltip("Prefab usato come icona quando il tracciato ha una previewImage. Deve avere un SpriteRenderer nei figli.")]
    public GameObject iconWithSpritePrefab;

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

        for (int i = 0; i < tracks.Count; i++)
        {
            TrackSceneData trackData = tracks[i];

            GameObject logicGO = new GameObject($"Track{i}_Button");
            logicGO.transform.SetParent(transform);

            TrackSelectionButton btn = logicGO.AddComponent<TrackSelectionButton>();
            btn.trackIndex = i;
            btn.nextGroupComponent = nextGroupComponent;

            UI_GraphicComponent graphic = new UI_GraphicComponent();
            graphic.Panel = panelPrefab;
            graphic.TextGUI = textPrefab;
            graphic.text = trackData.displayName;

            bool hasImage = trackData.previewImage != null && iconWithSpritePrefab != null;
            if (hasImage)
            {
                graphic.Icon = iconWithSpritePrefab;
                graphic.previewSprite = trackData.previewImage;
                graphic.TextOffset = new Vector3(-0.5f, -0.6f, -0.2f);
            }
            else
            {
                graphic.Icon = null;
                graphic.TextOffset = new Vector3(0f, 0f, -0.2f);
            }

            UI_Component_3D component = new UI_Component_3D();
            component.ComponentName = trackData.displayName;
            component.GraphicComponent = graphic;
            component.LogicComponent = btn;

            _group.UIComponentList.Add(component);
        }

        Debug.Log($"[DynamicTrackGroupPopulator] Populated {tracks.Count} tracks");
    }
}
