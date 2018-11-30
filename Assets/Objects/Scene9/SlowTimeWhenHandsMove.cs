using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowTimeWhenHandsMove : MonoBehaviour
{

    ControllerAccessors[] controllers;
    SetTime timeManager;
    public float minSpeed = 0.05f;
    public float maxSpeed = 0.25f;

    private float goalTime, currentTime, timeSlewDown, timeSlewUp;

    // Use this for initialization
    void Start()
    {
        controllers = transform.parent.GetComponentsInChildren<ControllerAccessors>();
        timeManager = GetComponent<SetTime>();
        goalTime = currentTime = 1;
        timeSlewDown = 0.1f;
        timeSlewUp = 0.01f;
    }

    // Update is called once per frame
    void Update()
    {
        float speed = 0;
        foreach( ControllerAccessors c in controllers ) { speed += c.Velocity().magnitude; }
        goalTime = speed.PowMapClamp( 0, 2, maxSpeed, minSpeed, pow: 1f );
        if( goalTime < currentTime )
        {
            currentTime += timeSlewDown * ( goalTime - currentTime );
        }
        else
        {
            currentTime += timeSlewUp * ( goalTime - currentTime );
        }
        timeManager.Scale( currentTime );
    }
}
