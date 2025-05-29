using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Speedometer : MonoBehaviour
{
    public float speedKmh = 0f;

    private Vector3 lastPosition;
    private Queue<float> lastSpeeds = new Queue<float>();
    private const int maxSamples = 3;

    private const float updateInterval = 0.1f; // 0.1 secondi

    void Start()
    {
        lastPosition = transform.position;
        StartCoroutine(UpdateSpeedCoroutine());
    }

    IEnumerator UpdateSpeedCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);

            Vector3 currentPosition = transform.position;
            float distance = Vector3.Distance(currentPosition, lastPosition);
            float speedMps = distance / updateInterval;
            float currentSpeedKmh = speedMps * 3.6f;

            lastSpeeds.Enqueue(currentSpeedKmh);
            if (lastSpeeds.Count > maxSamples)
            {
                lastSpeeds.Dequeue();
            }

            float sum = 0f;
            foreach (float s in lastSpeeds)
            {
                sum += s;
            }
            speedKmh = sum / lastSpeeds.Count;

            lastPosition = currentPosition;
        }
    }
}
