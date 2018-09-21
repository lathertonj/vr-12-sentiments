using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateClouds2 : MonoBehaviour
{
    public Vector3 rotationAmounts;
    private ControllerAccessors myController;
    private float maxSqueezeTime;

    private void Start()
    {
        myController = GetComponentInParent<ControllerAccessors>();
        // I am lazy
        maxSqueezeTime = ( (Scene5SeedlingController) FindObjectOfType( typeof( Scene5SeedlingController ) ) ).maxSqueezeTime;
    }

    void Update()
    {
        float multiplier = 1;
        // multiplier from velocity: up to 4x speed if controller moving fast enough
        // multiplier = myController.Velocity().magnitude.MapClamp( 0, 0.3f, 1, 4 );
        // multiplier from held down time: up to 7x speed if controller held long enough
        if( myController.IsSqueezed() )
        {
            multiplier = myController.ElapsedSqueezeTime().MapClamp( 0, maxSqueezeTime, 1, 7 );
        }

        transform.rotation *= 
              Quaternion.AngleAxis( Time.deltaTime * multiplier * rotationAmounts.x, Vector3.left )
            * Quaternion.AngleAxis( Time.deltaTime * multiplier * rotationAmounts.z, Vector3.forward )
            * Quaternion.AngleAxis( Time.deltaTime * multiplier * rotationAmounts.y, Vector3.up );        
    }
}
