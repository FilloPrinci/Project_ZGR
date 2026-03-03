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


    private float deltaTime;

    private void Awake()
    {

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

        
    }

    private void LateUpdate()
    {
        ApplyScale();
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

        
    }

    private void ApplyScale()
    {
        // Smoothly interpolate the effect scale towards the desired scale
        float currentScale = engineEffectModel.transform.localScale.y; // Assuming uniform scaling
        desideredEffectScale *= scaleMultiplier; // Apply the scale multiplier
        float newScale = Mathf.Lerp(currentScale, desideredEffectScale, deltaTime * 5f); // Adjust the speed of interpolation as needed
        engineEffectModel.transform.localScale = new Vector3(1, newScale, 1);
    }


}
