using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NamesData
{
    public List<string> names;
}

public class NamesLoader : MonoBehaviour
{
    [Header("JSON File")]
    public TextAsset jsonFile;

    public List<string> namesList;

    void Awake()
    {
        LoadNames();
    }

    public void LoadNames()
    {
        if (jsonFile == null)
        {
            Debug.LogError("JSON file non assegnato!");
            return;
        }

        NamesData data = JsonUtility.FromJson<NamesData>(jsonFile.text);

        if (data != null && data.names != null)
        {
            namesList = data.names;
            Debug.Log("Nomi caricati: " + namesList.Count);
        }
        else
        {
            Debug.LogError("Errore nel parsing del JSON");
        }
    }
}