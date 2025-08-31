using UnityEngine;

public enum CheckpointTypeEnum
{
    CornerStart,
    CornerMid,
    CornerEnd,
    Turbo,
    Item,
    Recharge
}

public class CheckpointType : MonoBehaviour
{
    public CheckpointTypeEnum checkpointType;

}
