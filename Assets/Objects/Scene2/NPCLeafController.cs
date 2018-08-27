using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLeafController : MonoBehaviour
{
    public Vector2 cycleRange;
    public Vector2 effectRange;
    public Vector3 effectDamping = Vector3.one;
    private Vector3 originalPosition;
    private float xCycle, yCycle, zCycle,
        xPhase, yPhase, zPhase,
        xEffectSize, yEffectSize, zEffectSize;

    // Use this for initialization
    void Start()
    {
        originalPosition = transform.position;

        xCycle = Random.Range( cycleRange.x, cycleRange.y );
        yCycle = Random.Range( cycleRange.x, cycleRange.y );
        zCycle = Random.Range( cycleRange.x, cycleRange.y );

        xPhase = Random.Range( 1, xCycle - 1 );
        yPhase = Random.Range( 1, yCycle - 1 );
        zPhase = Random.Range( 1, zCycle - 1 );

        xEffectSize = effectDamping.x * Random.Range( effectRange.x, effectRange.y );
        yEffectSize = effectDamping.y * Random.Range( effectRange.x, effectRange.y ) / 2;
        zEffectSize = effectDamping.z * Random.Range( effectRange.x, effectRange.y );
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = originalPosition + new Vector3(
            xEffectSize * Mathf.Cos( 2 * Mathf.PI * ( Time.time + xPhase ) / xCycle ),
            yEffectSize * Mathf.Cos( 2 * Mathf.PI * ( Time.time + yPhase ) / yCycle ),    
            zEffectSize * Mathf.Cos( 2 * Mathf.PI * ( Time.time + zPhase ) / zCycle )
        );
    }
}
