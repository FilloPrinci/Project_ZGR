using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class VehicleEffects : MonoBehaviour
{
    public GameObject particleEffect;
    public EngineFeedback engineFeedback;
    public EngineFeedback engineFeedback_2;
    public float particlePower = 0f;
    // Explicit turbo flag (set by FeedBackManager, same source PlayerSoundManager already uses
    // for engine audio) rather than inferring it from particlePower crossing some threshold -
    // particlePower caps at exactly 1f during turbo (0.5f * the 2f FeedBackManager uses), never
    // above, so EngineFeedback's own "currentEnginePower > 1f" boostMode check could never
    // actually fire through this pipeline.
    public bool onTurbo = false;
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
        }
    }

    // Update is called once per frame
    void Update()
    {
        ManageParticleSystems();
        ManageBoosterEffect();
    }

    void ManageBoosterEffect()
    {
        if (engineFeedback != null)
        {
            engineFeedback.currentEnginePower = particlePower;
            engineFeedback.boostMode = onTurbo;
        }

        if(engineFeedback_2 != null)
        {
            engineFeedback_2.currentEnginePower = particlePower;
            engineFeedback_2.boostMode = onTurbo;
        }
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
