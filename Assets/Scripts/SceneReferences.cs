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
