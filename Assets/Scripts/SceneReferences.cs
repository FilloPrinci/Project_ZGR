using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
public class SceneReferences : MonoBehaviour
{
    public static SceneReferences Instance { get; private set; }

    public string startScene;

    public string raceScene;

    public List<string> sceneNameList;

    public List<string> raceTrackSceneList;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogError("Duplicate SceneReferences detected. Destroying extra instance.");
            Destroy(gameObject); // Assicura che ci sia solo un SceneReferences
        }
    }

}
