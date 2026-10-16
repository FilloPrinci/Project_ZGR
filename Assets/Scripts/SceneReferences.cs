using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TrackSceneData
{
    public string sceneName;
    public string displayName;
    public string description;
    public Sprite previewImage;
}

[System.Serializable]
public class TrophyData
{
    public string displayName;
    public string description;
    public Sprite previewImage;

    // 3D model shown in the trophy selection menu instead of previewImage (see
    // DynamicTrophyGroupPopulator). iconSize is its uniform scale, iconOffset its position
    // relative to the menu panel.
    public GameObject iconPrefab;
    public float iconSize = 1f;
    public Vector3 iconOffset = new Vector3(0f, 0f, -0.2f);

    // Indices into SceneReferences.trackSceneDataList, in race order for this trophy.
    // Leave empty to mean "every track currently in trackSceneDataList, in their current
    // order" - this is what makes "Trofeo 1" automatically track whatever tracks exist
    // instead of needing its own hand-maintained, easy-to-desync copy of the track list.
    public List<int> trackIndexes;
}


public class SceneReferences : MonoBehaviour
{

    public static SceneReferences Instance { get; private set; }

    public string startScene;

    public string raceScene;

    // Thank-you screen + end credits, shown instead of going straight back to startScene once
    // a single race or a whole trophy is completed (see RaceGUI.OnRaceExit). Leave empty to skip.
    public string endingScene;

    public List<string> raceTrackSceneList;

    public List<TrackSceneData> trackSceneDataList;

    public List<TrophyData> trophyDataList;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("Duplicate SceneReferences detected. Destroying extra instance.");
            Destroy(gameObject); // Assicura che ci sia solo un SceneReferences
        }
    }
}
