using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float maxSpeedMultiplier = 1;
    public float maxRotationSpeedMultiplier = 1;
    public float accelerationMultiplier = 1;
    public float rotationAccelerationMultiplier = 1;

    public float speedItemMultiplier = 0.25f;
    public float accelerationItemMultiplier = 0.5f;
    public float rotationSpeedItemMultiplier = 0.25f;
    public float rotationAccelerationItemMultiplier = 0.5f;

    public float turboMaxSpeedMultiplier = 0.5f;
    public float turboAccelerationMultiplier = 1.0f;

    public Queue<ItemType> itemBuffer = new Queue<ItemType>();
    public int bufferSize = 3;

    public bool onTurbo = false;
    public int turboDuration = 2;

    void AddItemToBuffer(ItemType item)
    {
        itemBuffer.Enqueue(item);
        if (itemBuffer.Count > bufferSize)
        {
            itemBuffer.Dequeue();
        }
    }

    void ResetMultipliers()
    {
        maxSpeedMultiplier = 1;
        maxRotationSpeedMultiplier = 1;
        accelerationMultiplier = 1;
        rotationAccelerationMultiplier = 1;
    }

    void UpdateStats()
    {
        ResetMultipliers();

        // check if turbo should be applied
        if (onTurbo)
        {
            maxSpeedMultiplier += turboMaxSpeedMultiplier;
            accelerationMultiplier += turboAccelerationMultiplier;
        }

        // check for upgrade items to be applied

        int manUpgradeAmount = 0;
        int accUpgradeAmount = 0;
        int spdUpgradeAmount = 0;

        foreach (ItemType item in itemBuffer)
        {
            if (item == ItemType.UpgradeSpeed)
            {
                spdUpgradeAmount++;
            }
            else if (item == ItemType.UpgradeAcceleration)
            {
                accUpgradeAmount++;
            }
            else if (item == ItemType.UpgradeManeuverability)
            {
                manUpgradeAmount++;
            }
        }

        float rotationSpeedTotalMultiplier = manUpgradeAmount * rotationSpeedItemMultiplier;
        float rotationAccelerationTotalMultiplier = manUpgradeAmount * rotationAccelerationItemMultiplier;
        float speedTotalMultiplier = spdUpgradeAmount * speedItemMultiplier;
        float accelerationTotalMultiplier = accUpgradeAmount * accelerationItemMultiplier;

        maxSpeedMultiplier += speedTotalMultiplier;
        accelerationMultiplier += accelerationTotalMultiplier;
        maxRotationSpeedMultiplier += rotationSpeedTotalMultiplier;
        rotationAccelerationMultiplier += rotationAccelerationTotalMultiplier;
    }

    public void OnItemAcquired(ItemType type)
    {
        if (type == ItemType.UpgradeManeuverability || type == ItemType.UpgradeSpeed || type == ItemType.UpgradeAcceleration)
        {
            AddItemToBuffer(type);
            UpdateStats();
        }
    }

    public void StartTurbo()
    {
        StartCoroutine(TurboCoroutine());
    }

    private IEnumerator TurboCoroutine()
    {
        UpdateStats();
        onTurbo = true;
        yield return new WaitForSeconds(turboDuration);
        onTurbo = false;
        UpdateStats();
    }
}
