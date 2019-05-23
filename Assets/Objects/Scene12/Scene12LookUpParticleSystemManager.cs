using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene12LookUpParticleSystemManager : MonoBehaviour
{
    public ParticleSystem skyFascinator, windParticles;
    public static bool haveTransitioned = false;
    public float normalizedSizeCutoff = 0.4f;
    private ParticleSystem.EmissionModule fascinatorEmission, windEmission;

    void Start()
    {
        fascinatorEmission = skyFascinator.emission;
        windEmission = windParticles.emission;
        ChuckSubInstance myChuck = GetComponent<ChuckSubInstance>();
        gameObject.AddComponent<ChuckEventListener>().ListenForEvent( myChuck, "scene12StartWind", TurnOnWindParticles );
        gameObject.AddComponent<ChuckEventListener>().ListenForEvent( myChuck, "scene12StopWind", TurnOffWindParticles );
    }

    void TurnOnWindParticles()
    {
        windEmission.rateOverTime = 10;
    }

    void TurnOffWindParticles()
    {
        windEmission.rateOverTime = 0;
    }


    public void SetNormalizedSize( float normalizedSize )
    {
        if( !haveTransitioned && normalizedSize > normalizedSizeCutoff )
        {
            haveTransitioned = true;

            fascinatorEmission.rateOverTime = 0;
            windEmission.rateOverTime = 10;
        }
    }
}
