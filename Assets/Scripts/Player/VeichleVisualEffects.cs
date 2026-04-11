using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GlowEffectSettings
{
    public float intensity;
    public float fresnelPower;
    [ColorUsage(true, true)]
    public Color glowColor;
}

public class VeichleVisualEffects : MonoBehaviour
{
    [Header("Material Filter")]
    public Shader targetShader;
    public bool includeInactiveChildren = true;

    [Header("Collision Effect")]
    public GlowEffectSettings collisionEffectSettings;
    public GlowEffectSettings EnergyChargeEffectSettings;

    struct RendererSlot
    {
        public Renderer renderer;
        public int materialIndex;

        public RendererSlot(Renderer r, int i)
        {
            renderer = r;
            materialIndex = i;
        }
    }

    private List<RendererSlot> cachedSlots;
    private MaterialPropertyBlock block;

    void Awake()
    {
        if (targetShader == null)
        {
            Debug.LogWarning("Target shader is not assigned.");
            return;
        }

        block = new MaterialPropertyBlock();
        
    }

    private void Start()
    {
        cachedSlots = GetRendererSlots();
    }

    List<RendererSlot> GetRendererSlots()
    {
        List<RendererSlot> newCachedSlots = new List<RendererSlot>();

        Renderer[] renderers = GetComponentsInChildren<Renderer>(includeInactiveChildren);

        foreach (Renderer r in renderers)
        {
            Material[] mats = r.sharedMaterials;

            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = mats[i];
                if (mat == null) continue;

                if (mat.shader != null && targetShader != null &&
                    mat.shader.name == targetShader.name)
                {
                    newCachedSlots.Add(new RendererSlot(r, i));
                }
            }
        }

        if (newCachedSlots.Count == 0)
        {
            Debug.LogWarning("No renderers found with the target shader.");
        }

        return newCachedSlots;
    }

    public void SetGlow(float intensity,  float fresnel, Color color)
    {
        if (cachedSlots == null || cachedSlots.Count == 0)
        {
            // retry to cache slots if not done in Awake or if Awake was skipped
            cachedSlots = GetRendererSlots();
            if(cachedSlots == null ||  cachedSlots.Count == 0)
            {
                return;
            }
        }

        foreach (var slot in cachedSlots)
        {
            var r = slot.renderer;
            int i = slot.materialIndex;

            r.GetPropertyBlock(block, i);

            block.SetFloat("_Intensity", intensity);
            block.SetColor("_GlowColor", color);
            block.SetFloat("_FresnelPower", fresnel);

            r.SetPropertyBlock(block, i);
        }
    }

    public IEnumerator PlayCollisionEffect(float duration)
    {
        float elapsed = 0f;

        float maxIntensity = collisionEffectSettings.intensity;
        float maxFresnel = collisionEffectSettings.fresnelPower;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float intensity = Mathf.Lerp(maxIntensity, 0f, t);
            float fresnel = Mathf.Lerp(maxFresnel - 0.1f, maxFresnel, t);
            SetGlow(intensity, fresnel, collisionEffectSettings.glowColor);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // Ensure the effect is fully reset at the end
        SetGlow(0f, 0f, new Color(0, 0, 0));
    }

    public IEnumerator PlayEnergyChargeEffect(float duration)
    {
        float elapsed = 0f;

        float maxIntensity = EnergyChargeEffectSettings.intensity;
        float maxFresnel = EnergyChargeEffectSettings.fresnelPower;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float intensity = Mathf.Lerp(maxIntensity, 0f, t);
            float fresnel = Mathf.Lerp(maxFresnel - 0.1f, maxFresnel, t);
            SetGlow(intensity, fresnel, EnergyChargeEffectSettings.glowColor);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // Ensure the effect is fully reset at the end
        SetGlow(0f, 0f, new Color(0, 0, 0));
    }
}