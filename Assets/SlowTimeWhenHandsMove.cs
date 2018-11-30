using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowTimeWhenHandsMove : MonoBehaviour
{
	ControllerAccessors[] controllers;
	SetTime timeManager;
	public float minSpeed = 0.05f;
	public float maxSpeed = 0.5f;

    // Use this for initialization
    void Start()
    {
		controllers = transform.parent.GetComponentsInChildren<ControllerAccessors>();
		timeManager = GetComponent<SetTime>();
    }

    // Update is called once per frame
    void Update()
    {
		float speed = 0;
		foreach( ControllerAccessors c in controllers ) { speed += c.Velocity().magnitude; }
		timeManager.Scale( speed.PowMapClamp( 0, 10, minSpeed, maxSpeed, pow: 1f ) );
    }
}
