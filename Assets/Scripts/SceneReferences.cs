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


public class SceneReferences : MonoBehaviour
{
    
    public static SceneReferences Instance { get; private set; }

    public string startScene;

    public string raceScene;

    public List<string> raceTrackSceneList;

    public List<TrackSceneData> trackSceneDataList;

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
