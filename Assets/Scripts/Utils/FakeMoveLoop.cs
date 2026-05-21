using UnityEngine;

public class FakeMoveLoop : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Movement")]
    public Vector3 direction = Vector3.forward;
    public float speed = 10f;

    [Header("Loop")]
    public float maxDistance = 50f;

    private Vector3 startPosition;

    void Start()
    {
        if (target != null)
        {
            startPosition = target.position;
        }
    }

    void Update()
    {
        if (target == null)
            return;

        target.position += direction.normalized * speed * Time.deltaTime;

        float distance = Vector3.Distance(startPosition, target.position);

        if (distance >= maxDistance)
        {
            target.position = startPosition;
        }
    }
}
