using UnityEngine;
[System.Serializable]
public class Vehicle
{
    public GameObject vehiclePrefab;

    public Vehicle(GameObject vehiclePrefab)
    {
        this.vehiclePrefab = vehiclePrefab;
    }
}
