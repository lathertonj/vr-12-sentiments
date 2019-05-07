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

    private float currentSize;

    void Start()
    {
        room.localScale = startSize * Vector3.one;
        currentSize = startSize;
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
                currentSize += growthAdditionPerSecond * Time.deltaTime;
                break;
            case GrowthMethod.Multiplicative:
                float position = currentSize.MapClamp( startSize, endSize, 0, 1 );
                float extraMultiplier = growthMultiplierCurve.Evaluate( position );
                currentSize *= Mathf.Pow( 1 + growthMultiplicationPerSecond * extraMultiplier, Time.deltaTime );
                break;
        }

        if( currentSize > endSize )
        {
            currentSize = endSize;
        }
    }

    void NotGrow()
    {
        // do nothing?
    }
}

public enum GrowthMethod
{
    Additive,
    Multiplicative
}
