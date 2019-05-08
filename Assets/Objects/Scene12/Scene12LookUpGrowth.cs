using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene12LookUpGrowth : MonoBehaviour
{
    public Collider lookUpPlane;
    public Transform room;
    public float startSize = 0.07f;
    public float endSize;
    public GrowthMethod growthMethod;
    public float growthAdditionPerSecond = 0.1f;
    public float growthMultiplicationPerSecond = 0.1f;

    public AnimationCurve growthMultiplierCurve = AnimationCurve.EaseInOut( 0, 3, 1, 0 );

    public RotateSeedlings seedlingsToRotate;
    private Scene12LookUpParticleSystemManager particleManager;
    private float normalizedSize;
    private float sizeSlew = 2f;

    private float currentSize, goalSize;

    void Start()
    {
        room.localScale = startSize * Vector3.one;
        currentSize = goalSize = startSize;
        particleManager = GetComponent<Scene12LookUpParticleSystemManager>();
    }

    void Update()
    {
        RaycastHit hit;
        int layerMask = LayerMask.GetMask( "LookUpPlane" );
        if( Physics.Raycast( transform.position, transform.forward, out hit, Mathf.Infinity, layerMask ) )
        {
            if( hit.collider == lookUpPlane )
            {
                // grow!
                Grow();
            }
            else
            {
                // not grow!
                NotGrow();
            }

        }
        else
        {
            // not grow!
            NotGrow();
        }

        room.localScale = currentSize * Vector3.one;
    }

    void Grow()
    {
        switch( growthMethod )
        {
            case GrowthMethod.Additive:
                goalSize += growthAdditionPerSecond * Time.deltaTime;
                break;
            case GrowthMethod.Multiplicative:
                float position = currentSize.MapClamp( startSize, endSize, 0, 1 );
                float extraMultiplier = growthMultiplierCurve.Evaluate( position );
                goalSize *= Mathf.Pow( 1 + growthMultiplicationPerSecond * extraMultiplier, Time.deltaTime );
                break;
        }

        if( goalSize > endSize )
        {
            goalSize = endSize;
        }

        SlewSize();

        normalizedSize = currentSize.PowMapClamp( startSize, endSize, 0, 1, 0.6f );
        seedlingsToRotate.SetAmount( normalizedSize );
        particleManager.SetNormalizedSize( normalizedSize );
    }

    void NotGrow()
    {
        // when we are not growing, we are still slewing our size
        SlewSize();
    }

    void SlewSize()
    {
        currentSize += sizeSlew * Time.deltaTime * ( goalSize - currentSize );
    }
}



public enum GrowthMethod
{
    Additive,
    Multiplicative
}
