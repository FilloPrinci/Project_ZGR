using System.Collections.Generic;
using UnityEngine;

// Mirrors DynamicTrackGroupPopulator: builds the "Select trophy" menu group at runtime from
// SceneReferences.trophyDataList, so adding/removing a trophy never needs hand-wiring UI items.
[RequireComponent(typeof(UI_GroupComponent))]
public class DynamicTrophyGroupPopulator : MonoBehaviour
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
            Debug.LogError("[DynamicTrophyGroupPopulator] SceneReferences instance not found");
            return;
        }

        List<TrophyData> trophies = sceneRefs.trophyDataList;
        if (trophies == null || trophies.Count == 0)
        {
            Debug.LogError("[DynamicTrophyGroupPopulator] trophyDataList is empty");
            return;
        }

        _group.UIComponentList = new List<UI_Component_3D>();

        try
        {
            for (int i = 0; i < trophies.Count; i++)
            {
                TrophyData trophyData = trophies[i];

                GameObject logicGO = new GameObject($"Trophy{i}_Button");
                logicGO.transform.SetParent(transform);

                TrophySelectionButton btn = logicGO.AddComponent<TrophySelectionButton>();
                btn.trophyIndex = i;
                btn.nextGroupComponent = nextGroupComponent;

                bool hasImage = trophyData.previewImage != null;

                UI_GraphicComponent graphic = new UI_GraphicComponent();
                graphic.Panel = panelPrefab;
                graphic.Icon = null;
                graphic.TextGUI = null;

                if (hasImage)
                {
                    Sprite sprite = trophyData.previewImage;
                    graphic.previewSprite = sprite;
                    graphic.IconOffset = imageOffset;

                    float spriteWorldWidth = sprite.bounds.size.x;
                    graphic.IconSize = spriteWorldWidth > 0f ? targetImageWorldSize / spriteWorldWidth : 1f;
                }

                UI_Component_3D component = new UI_Component_3D();
                component.ComponentName = trophyData.displayName;
                component.GraphicComponent = graphic;
                component.LogicComponent = btn;

                _group.UIComponentList.Add(component);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DynamicTrophyGroupPopulator] Eccezione durante la popolazione: {e}");
        }

        Debug.Log($"[DynamicTrophyGroupPopulator] Populated {_group.UIComponentList.Count}/{trophies.Count} trophies");
    }
}
