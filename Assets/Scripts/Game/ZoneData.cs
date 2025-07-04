using UnityEngine;

public enum ZoneType
{
    Undefined,
    Turbo,
    Recharge,
    Damage
}

public class ZoneData : MonoBehaviour
{
    public ZoneType Type;
}
