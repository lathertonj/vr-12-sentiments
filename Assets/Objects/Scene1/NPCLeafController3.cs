using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLeafController3 : MonoBehaviour
{
    public Transform myHeadLeaf;
    public Vector2 effectRange;
    public Vector3 effectDamping;
    public float xCycle, zCycle,
        xPhase, zPhase;
    private float xEffectSize, yEffectSize, zEffectSize;
    public float upDelay;

    public float upTime;
    private float downTime;
    private float lastUpTime, lastDownTime;
    private bool goingUp = false;

    // Use this for initialization
    void Start()
    {
        downTime = upTime + 5;

        // don't remember what this is for
        // lastUpTime = Random.Range( -upTime + 1, -1 ); 
        lastUpTime = 0;
        lastDownTime = -upDelay;

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

        // map to angle
        float headAngle = height.Map( -1, 1, 55, 10 );
        Vector3 localEulerAngles = myHeadLeaf.localEulerAngles;
        localEulerAngles.z = headAngle;
        myHeadLeaf.localEulerAngles = localEulerAngles;
    }
}
