using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateClouds : MonoBehaviour
{
    public Vector3 rotationAmounts;
    private ControllerAccessors myController;

    private void Start()
    {
        myController = GetComponentInParent<ControllerAccessors>();
    }

    void Update()
    {
        // up to 4x speed if controller moving fast enough
        float multiplier = myController.Velocity().magnitude.MapClamp( 0, 0.3f, 1, 4 );
        transform.rotation *= 
              Quaternion.AngleAxis( Time.deltaTime * multiplier * rotationAmounts.x, Vector3.left )
            * Quaternion.AngleAxis( Time.deltaTime * multiplier * rotationAmounts.z, Vector3.forward )
            * Quaternion.AngleAxis( Time.deltaTime * multiplier * rotationAmounts.y, Vector3.up ) ;
    }
}
