using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLeafController2 : MonoBehaviour
{
    public Vector2 cycleRange;
    public Vector2 effectRange;
    public Vector3 effectDamping;
    private float xCycle, zCycle,
        xPhase, zPhase,
        xEffectSize, yEffectSize, zEffectSize;

    private float upTime, downTime;
    private float lastUpTime, lastDownTime;
    private bool goingUp = true;

    // Use this for initialization
    void Start()
    {
        xCycle = Random.Range( cycleRange.x, cycleRange.y );
        zCycle = Random.Range( cycleRange.x, cycleRange.y );
        upTime = Random.Range( cycleRange.x, cycleRange.y );
        downTime = upTime + 5;

        xPhase = Random.Range( 1, xCycle - 1 );
        zPhase = Random.Range( 1, zCycle - 1 );
        lastUpTime = Random.Range( -upTime + 1, -1 ); 
        lastDownTime = 0;

        xEffectSize = effectDamping.x * Random.Range( effectRange.x, effectRange.y );
        yEffectSize = effectDamping.y * Random.Range( effectRange.x, effectRange.y ) / 2;
        zEffectSize = effectDamping.z * Random.Range( effectRange.x, effectRange.y );
    }

    // Update is called once per frame
    void Update()
    {
        float height = 0;
        if( goingUp )
        {
            float elapsedTime = Time.time - lastUpTime;
            height = elapsedTime / upTime;
            if( elapsedTime > upTime )
            {
                goingUp = false;
                lastDownTime = Time.time;
            }
        }
        else
        {
            float elapsedTime = Time.time - lastDownTime;
            height = 1 - elapsedTime / downTime;
            if( elapsedTime > downTime )
            {
                goingUp = true;
                lastUpTime = Time.time;
            }
        }
        // map from linear [0, 1] to curvy [-1, 1]
        height = -Mathf.Cos( Mathf.PI * Mathf.Clamp01( height ) );
        transform.localPosition = new Vector3(
            xEffectSize * Mathf.Cos( 2 * Mathf.PI * ( Time.time + xPhase ) / xCycle ),
            yEffectSize * height,    
            zEffectSize * Mathf.Cos( 2 * Mathf.PI * ( Time.time + zPhase ) / zCycle )
        );
    }
}
