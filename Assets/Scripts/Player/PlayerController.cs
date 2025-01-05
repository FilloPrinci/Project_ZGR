using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float acceleration = 50f;     // Velocità di accelerazione
    public float maxSpeed = 200f;        // Velocità massima
    public float turnSpeed = 150f;       // Velocità di rotazione
    public float orientationSpeed = 10f; // Velocità di orientamento
    public float gravity = 9.81f;        // Gravità personalizzata
    public float hoverHeight = 2f;       // Altezza di sospensione del veicolo
    public float hoverForce = 500f;      // Forza che mantiene il veicolo sospeso
    public float brakingFactor = 0.1f;      // Riduzione della velocità naturale
    public float inputSmoothing = 5f;      // Fattore di smussamento degli input
    public float hoverForceMultiplier = 5f; // Fattore di forza del mantenimento altezza
    public float maxHoverForce = 200f; // Limite massimo della forza di mantenimento


    private Vector3 velocity; // Velocità del veicolo
    private Vector3 groundNormal = Vector3.up; // Normale alla superficie attuale

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        ApplyGravityAndHover();
        HandleMovement();
    }

    void HandleInput()
    {
        float forwardInput = Input.GetAxis("Vertical"); // W/S o frecce su/giù
        float turnInput = Input.GetAxis("Horizontal");  // A/D o frecce sinistra/destra

        // Smussa il cambiamento di input per evitare oscillazioni improvvise
        float smoothAcceleration = Mathf.Lerp(0, forwardInput, Time.deltaTime * inputSmoothing);
        Vector3 forwardForce = transform.forward * smoothAcceleration * acceleration * Time.deltaTime;

        // Modifica graduale della velocità
        velocity += forwardForce;

        // Rotazione
        float turn = Mathf.Lerp(0, turnInput * turnSpeed, Time.deltaTime * inputSmoothing);
        transform.Rotate(0, turn, 0);
    }

    void HandleMovement()
    {
        // Calcola la velocità locale rispetto alla direzione del veicolo
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        // Limita la velocità massima
        localVelocity.z = Mathf.Clamp(localVelocity.z, -maxSpeed, maxSpeed);
        velocity = transform.TransformDirection(localVelocity);

        // Simula una frenata naturale
        velocity = Vector3.Lerp(velocity, Vector3.zero, brakingFactor * Time.deltaTime);

        // Applica la velocità al veicolo
        transform.position += velocity * Time.deltaTime;
    }

    void ApplyGravityAndHover()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, hoverHeight * 2))
        {
            float hoverError = hoverHeight - hit.distance;

            // Calcola una forza di mantenimento dell'altezza
            Vector3 hoverForce = groundNormal * hoverError * hoverForceMultiplier;

            // Evita oscillazioni introducendo un limitatore alla velocità di correzione verticale
            velocity += Vector3.ClampMagnitude(hoverForce, maxHoverForce * Time.deltaTime);

            // Adatta la velocità di discesa/gravitazione solo se non in hover
            velocity += Vector3.Project(Vector3.down * gravity, groundNormal) * Time.fixedDeltaTime;
        }
    }
}
