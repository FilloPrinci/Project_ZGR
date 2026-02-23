using UnityEngine;

public class EngineFeedback : MonoBehaviour
{

    [Header("Visual Effects")]
    public GameObject engineEffectModel;
    public float minEffectScale = 0.1f;
    public float normalEffectScale = 1f;
    public float maxEffectScale = 2f;

    public float scaleMultiplier = 1f; // This can be used to adjust the overall scale of the effect
    public float currentEnginePower = 0f; // This should be set based on the player's current engine power (0 to 1)

    public bool boostMode = false;
    private float desideredEffectScale = 1f;

    [Header("Audio Effects")]
    public AudioSource engineAudioSource;
    public float baseAudioVolume = 1f; // Base volume for the engine sound

    private float baseAudioPitch = 1f; // Base pitch for the engine sound
    private float desideredAudioPitch = 1f; // Desired pitch based on engine power
    private float desideredAudioVolume = 1f; // Desired volume based on engine power

    private float deltaTime;

    private void Awake()
    {
        if (engineAudioSource != null)
        {
            float randomPitch = Random.Range(0.01f, 0.1f);

            baseAudioPitch += randomPitch;

            engineAudioSource.volume = baseAudioVolume;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


        
    }

    // Update is called once per frame
    void Update()
    {
        deltaTime = Time.deltaTime;

        GetEffectsReady();

        ApplyScale();
        ApplyAudioEffects();
    }

    void GetEffectsReady()
    {
        // Visual Effects

        if (currentEnginePower > 1f)
        {
            boostMode = true;
        }
        else
        {
            boostMode = false;
        }

        if (!boostMode)
        {
            currentEnginePower = Mathf.Clamp01(currentEnginePower); // Ensure engine power is between 0 and 1
            desideredEffectScale = Mathf.Lerp(minEffectScale, normalEffectScale, currentEnginePower);
        }
        else
        {
            desideredEffectScale = maxEffectScale;
        }

        // Audio Effects

        if (!boostMode)
        {
            desideredAudioPitch = Mathf.Lerp(baseAudioPitch, baseAudioPitch * 2f, currentEnginePower); // Adjust the pitch range as needed
            desideredAudioVolume = Mathf.Lerp(baseAudioVolume * 0.5f, baseAudioVolume, currentEnginePower); // Adjust volume based on engine power
        }
        else
        {
            desideredAudioPitch = baseAudioPitch * 2f; // Max pitch in boost mode
            desideredAudioVolume = baseAudioVolume; // Max volume in boost mode
        }
    }

    private void ApplyScale()
    {
        // Smoothly interpolate the effect scale towards the desired scale
        float currentScale = engineEffectModel.transform.localScale.y; // Assuming uniform scaling
        desideredEffectScale *= scaleMultiplier; // Apply the scale multiplier
        float newScale = Mathf.Lerp(currentScale, desideredEffectScale, deltaTime * 5f); // Adjust the speed of interpolation as needed
        engineEffectModel.transform.localScale = new Vector3(1, newScale, 1);
    }

    private void ApplyAudioEffects()
    {
        if(engineAudioSource != null)
        {
            float currentAudioPitch = engineAudioSource.pitch;
            desideredAudioPitch = baseAudioPitch + currentEnginePower; // Adjust pitch based on boost mode and engine power
            desideredAudioPitch = Mathf.Lerp(currentAudioPitch, desideredAudioPitch, deltaTime * 5f); // Smoothly interpolate pitch

            engineAudioSource.pitch = desideredAudioPitch;

            float currentAudioVolume = engineAudioSource.volume;
            desideredAudioVolume = baseAudioVolume + currentEnginePower; // Adjust volume based on boost mode and engine power
            desideredAudioVolume = Mathf.Lerp(currentAudioVolume, desideredAudioVolume, deltaTime * 5f); // Smoothly interpolate volume

            engineAudioSource.volume = desideredAudioVolume;
        }
    }

}
