using UnityEngine;

public class EngineFeedback : MonoBehaviour
{

    [Header("Visual Effects")]
    public GameObject engineEffectModel;
    public GameObject powerOnEffect;
    public float minEffectScale = 0.1f;
    public float normalEffectScale = 1f;
    public float maxEffectScale = 2f;

    public float scaleMultiplier = 1f; // This can be used to adjust the overall scale of the effect
    public float currentEnginePower = 0f; // This should be set based on the player's current engine power (0 to 1)

    public bool boostMode = false;
    private float desideredEffectScale = 1f;

    [Header("Booster Color")]
    // The booster plane's own BoosterEffect shadergraph only reads _BaseColor (tint) and
    // _Emission (HDR glow) - confirmed in the shadergraph asset (the only two properties with
    // m_GeneratePropertyBlock: true); the material's _Color is an unused Lit-template leftover.
    // Driven via MaterialPropertyBlock rather than a material instance, same reasoning as
    // VehicleVisualEffects's glow (CLAUDE.md: no material instances, safe for performance).
    public Color normalBoosterColor = Color.white;
    public Color turboBoosterColor = new Color(1f, 0.7529412f, 0f, 1f); // same yellow as Turbo.mat/TurboPlane.mat
    public float boosterEmissionIntensity = 12f; // matches BoosterMaterial's authored white _Emission magnitude
    public float boosterColorDecay = 8f;

    private Renderer engineEffectRenderer;
    private MaterialPropertyBlock boosterColorBlock;
    private Color currentBoosterColor;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionId = Shader.PropertyToID("_Emission");

    private float deltaTime;

    private void Awake()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (engineEffectModel != null)
        {
            engineEffectRenderer = engineEffectModel.GetComponentInChildren<Renderer>();
        }

        if (engineEffectRenderer != null)
        {
            boosterColorBlock = new MaterialPropertyBlock();
            currentBoosterColor = normalBoosterColor;
            ApplyBoosterColor(currentBoosterColor, 0f);
        }
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
        ApplyBoosterColorForFrame();
    }

    void GetEffectsReady()
    {
        // Visual Effects

        // boostMode is now set externally by VehicleEffects.ManageBoosterEffect(), from
        // PlayerStats.onTurbo via FeedBackManager - see the field's own comment for why inferring
        // it from currentEnginePower here never actually fired.

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

    private void ApplyBoosterColorForFrame()
    {
        if (engineEffectRenderer == null) return;

        Color targetColor = boostMode ? turboBoosterColor : normalBoosterColor;
        currentBoosterColor = Utils.ExpDecay(currentBoosterColor, targetColor, boosterColorDecay, deltaTime);

        // Emission magnitude follows engine power (0 at idle, full at boostMode) rather than
        // always sitting at boosterEmissionIntensity - confirmed live that was the glow looking
        // "always too bright" even at idle, since it had nothing to do with how hard the engine
        // was actually running (only the model's own scale did).
        float powerFraction = boostMode ? 1f : currentEnginePower;
        ApplyBoosterColor(currentBoosterColor, powerFraction);
    }

    private void ApplyBoosterColor(Color color, float emissionPowerFraction)
    {
        // Built manually rather than "color * intensity" - Color's own operator* also scales
        // alpha, and BoosterMaterial's authored _Emission has alpha 0 (unlike _BaseColor's 1),
        // which that would clobber to a nonzero value instead.
        float intensity = boosterEmissionIntensity * emissionPowerFraction;
        Color emission = new Color(color.r * intensity, color.g * intensity, color.b * intensity, 0f);

        engineEffectRenderer.GetPropertyBlock(boosterColorBlock);
        boosterColorBlock.SetColor(BaseColorId, color);
        boosterColorBlock.SetColor(EmissionId, emission);
        engineEffectRenderer.SetPropertyBlock(boosterColorBlock);
    }
}
