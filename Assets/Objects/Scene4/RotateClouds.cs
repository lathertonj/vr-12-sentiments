using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateClouds : MonoBehaviour
{
    public Vector3 rotationAmounts;
    private ControllerAccessors myController;
    public float maxSqueezeTime = 5f;
    private float currentMultiplier;
    private float multiplierSlew;

    private void Start()
    {
        myController = GetComponentInParent<ControllerAccessors>();
        currentMultiplier = 1;
    }

    void Update()
    {
        float goalMultiplier = 1;
        float multiplierSlew = 1 * Time.deltaTime;
        // multiplier from velocity: up to 4x speed if controller moving fast enough
        // multiplier = myController.Velocity().magnitude.MapClamp( 0, 0.3f, 1, 4 );
        // multiplier from held down time: up to 7x speed if controller held long enough
        if( myController.IsSqueezed() )
        {
            goalMultiplier = myController.ElapsedSqueezeTime().MapClamp( 0, maxSqueezeTime, 1, 7 );
        }
        currentMultiplier += multiplierSlew * ( goalMultiplier - currentMultiplier );

        transform.rotation *= 
              Quaternion.AngleAxis( Time.deltaTime * currentMultiplier * rotationAmounts.x, Vector3.left )
            * Quaternion.AngleAxis( Time.deltaTime * currentMultiplier * rotationAmounts.z, Vector3.forward )
            * Quaternion.AngleAxis( Time.deltaTime * currentMultiplier * rotationAmounts.y, Vector3.up );        
    }
}
