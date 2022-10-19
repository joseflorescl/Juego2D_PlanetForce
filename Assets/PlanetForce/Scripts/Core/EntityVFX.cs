using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityVFX : MonoBehaviour
{
    [SerializeField] private GameObject vfxDead;
    [SerializeField] private GameObject vfxDamage;

    ParticleSystem[] vfxDeadParticles;
    ParticleSystem[] vfxDamageParticles;

    private void Awake()
    {
        if (vfxDead)
            vfxDeadParticles = vfxDead.GetComponentsInChildren<ParticleSystem>();
        if (vfxDamage)
            vfxDamageParticles = vfxDamage.GetComponentsInChildren<ParticleSystem>();
    }

    public void PlayDead()
    {
        if (vfxDeadParticles == null || vfxDeadParticles.Length == 0) return;
        PlayParticles(vfxDeadParticles);
    }

    public void PlayDamage()
    {
        if (vfxDamageParticles == null || vfxDamageParticles.Length == 0) return;
        PlayParticles(vfxDamageParticles);
    }

    void PlayParticles(ParticleSystem[] particles)
    {
        for (int i = 0; i < particles.Length; i++)
        {
            var ps = particles[i];
            ps.Stop();
            ps.Play();            
        }
    }
}
