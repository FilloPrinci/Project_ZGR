using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private float verticalInput;
    private float horizontalInput;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Duplicate InputManager detected. Destroying extra instance.");
            Destroy(gameObject); // Assicura che ci sia solo un InputManager
        }
    }


    // Update is called once per frame
    void Update()
    {
        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");
    }

    public bool accellerate() {
        return verticalInput > 0;
    }

    public bool brake() {
        return verticalInput < 0;
    }

    public float steer() {
        return horizontalInput;
    }
}
