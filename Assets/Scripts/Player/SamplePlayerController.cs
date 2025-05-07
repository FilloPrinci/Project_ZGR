using UnityEngine;
using UnityEngine.InputSystem;
public class SamplePlayerController : MonoBehaviour
{
    private Vector2 moveInput;
    public GameObject player;
    private Rigidbody rb;

    public float speed = 5f;

    void Awake()
    {
        rb = player.GetComponent<Rigidbody>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        rb.AddForce(move * speed);
    }
}
