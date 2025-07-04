using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    public float rotationSpeed;
    public Vector3 rotationAxis;


    void Update()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
        
    }
}
