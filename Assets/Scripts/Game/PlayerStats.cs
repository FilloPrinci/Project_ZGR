using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PlayerStats : MonoBehaviour
{
    public int hitDamage = 20;
    public int energyRecharge = 5;
    public float energyZoneRecharge = 0.01f;

    public float maxSpeedKmH = 300f;
    public float maxRotationSpeed = 10f;
    public float acceleration = 5f;
    public float rotationAcceleration = 5f;

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

    private float currentMaxSpeed;
    private float currentRotationSpeed;
    private float currentAcceleration;
    private float currentRotationAcceleration;

    private float energy = 0;

    public float CurrentMaxSpeed { get => currentMaxSpeed; set => currentMaxSpeed = value; }
    public float CurrentRotationSpeed { get => currentRotationSpeed; set => currentRotationSpeed = value; }
    public float CurrentAcceleration { get => currentAcceleration; set => currentAcceleration = value; }
    public float CurrentRotationAcceleration { get => currentRotationAcceleration; set => currentRotationAcceleration = value; }
    public float Energy { get => energy; set => energy = value; }

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

    void UpdateEnergy(float value)
    {
        Debug.Log("Updating energy by " + value);

        float energyResult = energy + value;

        if (energyResult < 0)
        {
            energy = 0;
        }else if (energyResult > 100)
        {
            energy = 100;
        }
        else{
            energy = energyResult;
        }

        Debug.Log("Current energy: " + energy);
    }

    void ApplyStats()
    {
        currentMaxSpeed = maxSpeedKmH / 3.6f * maxSpeedMultiplier;
        currentAcceleration = acceleration * accelerationMultiplier;
        currentRotationSpeed = maxRotationSpeed * maxRotationSpeedMultiplier;
        currentRotationAcceleration = rotationAcceleration * rotationAccelerationMultiplier;
    }

    private void Start()
    {
        UpdateStats();
        ApplyStats();
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

        ItemType[] items = itemBuffer.ToArray();

        for (int i = 0; i < items.Length; i++)
        {
            ItemType item = items[i];

            int energyRequired = (i + 1) * 20;

            if (item == ItemType.UpgradeSpeed && energy >= energyRequired)
            {
                spdUpgradeAmount++;
            }
            else if (item == ItemType.UpgradeAcceleration && energy >= energyRequired)
            {
                accUpgradeAmount++;
            }
            else if (item == ItemType.UpgradeManeuverability && energy >= energyRequired)
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

        ApplyStats();
    }

    public void OnItemAcquired(ItemType type)
    {
        if (type == ItemType.UpgradeManeuverability || type == ItemType.UpgradeSpeed || type == ItemType.UpgradeAcceleration)
        {
            AddItemToBuffer(type);
            UpdateStats();
        }else if (type == ItemType.EnergyRecharge)
        {
            Debug.Log("Energy acquired");
            UpdateEnergy(energyRecharge);
            UpdateStats();
        }
    }

    public void OnEnergyRechargeBySpeed(float deltaTime, float currentSpeed)
    {
        Debug.Log("Recharging energy...");
        float energyToCharge = energyZoneRecharge * currentSpeed * deltaTime;
        UpdateEnergy(energyToCharge);
        UpdateStats();
    }

    public void StartTurbo()
    {
        StartCoroutine(TurboCoroutine());
    }

    public void OnDamage()
    {
        UpdateEnergy(-hitDamage);
        UpdateStats();
    }

    private IEnumerator TurboCoroutine()
    {
        onTurbo = true;
        UpdateStats();
        yield return new WaitForSeconds(turboDuration);
        onTurbo = false;
        UpdateStats();
    }
}
