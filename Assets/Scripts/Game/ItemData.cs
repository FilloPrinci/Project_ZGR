using UnityEngine;

public enum ItemType
{
    Undefined,
    Turbo,
    UpgradeSpeed,
    UpgradeAcceleration,
    UpgradeManeuverability,
    EnergyRecharge
}

public class ItemData : MonoBehaviour
{
    public ItemType Type;
}
