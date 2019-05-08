using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene12LookUpParticleSystemManager : MonoBehaviour
{
    public ParticleSystem skyFascinator, windParticles;
    private bool haveTransitioned = false;
    public float normalizedSizeCutoff = 0.4f;


    public void SetNormalizedSize( float normalizedSize )
    {
        if( !haveTransitioned && normalizedSize > normalizedSizeCutoff )
        {
            haveTransitioned = true;
            ParticleSystem.EmissionModule fascinatorEmission = skyFascinator.emission;
            ParticleSystem.EmissionModule windParticleEmission = windParticles.emission;

            fascinatorEmission.rateOverTime = 0;
            windParticleEmission.rateOverTime = 10;
        }
    }
}
