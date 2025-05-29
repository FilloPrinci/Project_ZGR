using UnityEngine;

public enum ItemType
{
    Undefined,
    Turbo,
    UpgradeSpeed,
    UpgradeAcceleration,
    UpgradeManeuverability
}

public class ItemData : MonoBehaviour
{
    public ItemType Type;
}
