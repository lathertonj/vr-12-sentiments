using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSproutController : MonoBehaviour
{
    public Transform parentToEmbiggen;
    private ParticleSystem myParticles;
    private Vector3 prevPos;

    // Use this for initialization
    void Start()
    {
        prevPos = transform.position;
        myParticles = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentPos = transform.position;
        float heightChange = currentPos.y - prevPos.y;
        if( heightChange > 0 )
        {
            float scaleIncrease = heightChange.MapClamp( 0, 0.005f, 0, 0.0009f );
            float newScale = parentToEmbiggen.localScale.x + scaleIncrease;
            parentToEmbiggen.localScale = newScale * Vector3.one;

            if( !myParticles.isPlaying )
            {
                myParticles.Play();
            }
        }
        else
        {
            if( myParticles.isPlaying )
            {
                myParticles.Stop( false, ParticleSystemStopBehavior.StopEmitting );
            }
        }

        prevPos = currentPos;
    }
}
