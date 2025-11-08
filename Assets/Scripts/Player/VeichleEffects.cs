using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class VeichleEffects : MonoBehaviour
{
    public GameObject particleEffect;
    public float particlePower = 0f;
    private List<ParticleSystem> particleEffectList = new List<ParticleSystem>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (particleEffect != null)
        {
            particleEffectList.Clear();
            int childCount = particleEffect.transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                ParticleSystem particleSystem = particleEffect.transform.GetChild(i).gameObject.GetComponentInChildren<ParticleSystem>();

                if (particleSystem != null)
                {
                    particleEffectList.Add(particleSystem);
                }
            }

            if (particleEffectList.Count == 0)
            {
                Debug.LogWarning("No Particle Systems found in the specified particleEffect GameObject or its children.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
           ManageParticleSystems();
    }

    void ManageParticleSystems()
    {
        if(particleEffectList == null || particleEffectList.Count == 0)
        {
            return;
        }

        foreach (ParticleSystem ps in particleEffectList)
        {
            var main = ps.main;
            main.startSize = new ParticleSystem.MinMaxCurve(particlePower);
        }
    }
}
