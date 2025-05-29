using UnityEngine;
using System.Collections;

public class CountdownManager : MonoBehaviour
{
    public static CountdownManager Instance { get; private set; }

    public int count = 3;

    private Coroutine countdownCoroutine;
    private RaceManager raceManager;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        raceManager = RaceManager.Instance;
    }

    public void StartCountdown()
    {
        if (countdownCoroutine == null)
        {
            countdownCoroutine = StartCoroutine(CountdownCoroutine());
        }
    }

    private IEnumerator CountdownCoroutine()
    {
        while (count > 0)
        {
            Debug.Log("Countdown: " + count);
            yield return new WaitForSeconds(1f);
            count--;
        }

        Debug.Log("Countdown finished!");
        if(raceManager != null)
        {
            raceManager.TriggerRaceEvent(RacePhaseEvent.RaceStart);
        }
        countdownCoroutine = null;
    }
}
