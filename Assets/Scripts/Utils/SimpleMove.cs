using UnityEngine;

public class SimpleMove : MonoBehaviour
{
    public float speed = 10f;
    public Vector3 direction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition += direction * speed * Time.deltaTime;
    }
}
