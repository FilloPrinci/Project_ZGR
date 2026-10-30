using UnityEngine;

public class RandomPrefabSpawner : MonoBehaviour
{
    // Static: survives the scene reload, so returning to the title screen never shows the same
    // vehicle twice in a row.
    private static int lastSpawnedIndex = -1;

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

        int randomIndex = Utils.RandomIndexExcluding(prefabs.Length, lastSpawnedIndex);
        lastSpawnedIndex = randomIndex;

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
