using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private float verticalInput;
    private float horizontalInput;
    private float primaryButtonInput;
    private float steerDeadZone = 0.05f;

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
        primaryButtonInput = Input.GetAxis("Fire1");
        horizontalInput = Input.GetAxis("Horizontal");
    }

    public bool accellerate() {
        return primaryButtonInput > 0;
    }

    public bool brake() {
        return verticalInput < 0;
    }

    public float steer() {
        float steeringValue = 0;
        
        if (Mathf.Abs(horizontalInput) > steerDeadZone) {
            steeringValue = horizontalInput;
        }

        return steeringValue;
    }
}
