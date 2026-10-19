using UnityEngine;

// Thin blue "speed lines" streaks around the vehicle, driven by both the player's current energy
// (PlayerStats.Energy, 0-100 - the same value behind the HUD energy bar, and in
// PowerUpMode.energyOnlyStats the direct source of the car's extra max speed) and how much of the
// car's current max speed it's actually using right now: invisible standing still or at 0 energy,
// full density/speed once both are near their max. The vehicle moving forward near top speed
// also pushes the effect further ahead of it (not just more intense) so it actually reads at a
// glance instead of hiding behind/under the vehicle body.
//
// Lives on VehiclePivot (specifically on its "SpeedLines" child, alongside the ParticleSystem
// itself - so both the particle system and its tunable ranges are in one place) rather than the
// per-model vehicle prefab (where EngineFeedback/VehicleEffects live): like VehicleVisualEffects's
// collision/energy glow, this is a generic feedback effect independent of which vehicle model is
// spawned as VehiclePivot's child, so it only needs setting up once instead of once per vehicle.
public class SpeedLinesEffect : MonoBehaviour
{
    [Header("Particle System")]
    [Tooltip("Defaults to the ParticleSystem on this same GameObject if left unassigned.")]
    public ParticleSystem speedLines;

    [Header("Emission (0 -> maxEmissionRate at full energy+speed)")]
    public float minEmissionRate = 0f;
    public float maxEmissionRate = 40f;

    [Header("Particle speed (0 -> maxParticleSpeed at full energy+speed)")]
    public float minParticleSpeed = 2f;
    public float maxParticleSpeed = 12f;

    [Header("Vehicle speed gate")]
    [Tooltip("Fraction of the vehicle's current max speed at which the effect already reaches full intensity - full throttle rarely sits exactly at 100%, so this saturates a bit early (e.g. 0.85 = full effect from 85% of max speed up).")]
    [Range(0.1f, 1f)]
    public float fullEffectSpeedFraction = 0.85f;

    [Header("Forward offset (faster = further ahead of the vehicle, so it stays readable)")]
    public float minForwardOffset = 0f;
    public float maxForwardOffset = 4f;

    [Header("Smoothing")]
    public float decay = 6f;

    private float currentIntensityFraction = 0f;
    private float currentPositionFraction = 0f;
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.VelocityOverLifetimeModule velocityModule;

    private void Awake()
    {
        if (speedLines == null)
        {
            speedLines = GetComponent<ParticleSystem>();
        }

        if (speedLines != null)
        {
            emissionModule = speedLines.emission;
            velocityModule = speedLines.velocityOverLifetime;

            // Shape-driven initial velocity (main.startSpeed) points radially outward from the
            // ring (every direction around it, not front-to-back) - confirmed live that's what
            // made particles fly off in "random" directions. Zeroing it out and driving motion
            // only through velocityOverLifetime's local -Z (set below, regardless of where on
            // the ring a particle spawned) is what actually gets every particle moving uniformly
            // backward along the vehicle's own axis.
            ParticleSystem.MainModule mainModule = speedLines.main;
            mainModule.startSpeed = 0f;

            ParticleSystem.ShapeModule shapeModule = speedLines.shape;
            shapeModule.randomDirectionAmount = 0f;
        }
    }

    // energy0to100 matches PlayerStats.Energy's own range directly - no remapping needed at the
    // call site. currentSpeed/maxSpeed come straight from PlayerController.GetCurrentSpeed()/
    // GetMaxSpeed() - same units on both sides, so their ratio is all that matters.
    public void UpdateEffect(float energy0to100, float currentSpeed, float maxSpeed)
    {
        float energyFraction = Mathf.Clamp01(energy0to100 / 100f);
        float rawSpeedFraction = maxSpeed > 0f ? Mathf.Clamp01(currentSpeed / maxSpeed) : 0f;
        // Saturates before 100% of max speed (fullEffectSpeedFraction), so "near top speed" reads
        // as fully intense rather than needing the literal theoretical max.
        float speedGate = fullEffectSpeedFraction > 0f ? Mathf.Clamp01(rawSpeedFraction / fullEffectSpeedFraction) : rawSpeedFraction;

        // Standing still or empty energy both zero the effect out, not just reduce it - requires
        // both factors at once rather than either alone being enough.
        float targetIntensity = energyFraction * speedGate;
        currentIntensityFraction = Utils.ExpDecay(currentIntensityFraction, targetIntensity, decay, Time.deltaTime);

        // Positioning reacts to raw vehicle speed alone (not gated by energy) - how fast you're
        // actually going is what makes the effect hide behind the vehicle if left centered on it.
        currentPositionFraction = Utils.ExpDecay(currentPositionFraction, rawSpeedFraction, decay, Time.deltaTime);
        float forwardOffset = Mathf.Lerp(minForwardOffset, maxForwardOffset, currentPositionFraction);
        transform.localPosition = new Vector3(0f, 0f, forwardOffset);

        if (speedLines == null) return;

        emissionModule.rateOverTime = Mathf.Lerp(minEmissionRate, maxEmissionRate, currentIntensityFraction);

        float particleSpeed = Mathf.Lerp(minParticleSpeed, maxParticleSpeed, currentIntensityFraction);
        velocityModule.z = new ParticleSystem.MinMaxCurve(-particleSpeed); // local -Z: front-to-back, opposite the vehicle's forward
    }
}
