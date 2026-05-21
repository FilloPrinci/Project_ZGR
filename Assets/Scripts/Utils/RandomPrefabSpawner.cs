using UnityEngine;

public class RandomPrefabSpawner : MonoBehaviour
{
    public GameObject[] prefabs;

    public Transform parent;

    public bool resetLocalTransform = true;

    private void Start()
    {
        SpawnRandomPrefab();
    }

    public void SpawnRandomPrefab()
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("No assigned prefab!");
            return;
        }

        int randomIndex = Random.Range(0, prefabs.Length);

        GameObject selectedPrefab = prefabs[randomIndex];

        GameObject spawnedObject = Instantiate(selectedPrefab, parent);

        if (resetLocalTransform)
        {
            spawnedObject.transform.localPosition = Vector3.zero;
            spawnedObject.transform.localRotation = Quaternion.identity;
            spawnedObject.transform.localScale = Vector3.one;
        }
    }
}
