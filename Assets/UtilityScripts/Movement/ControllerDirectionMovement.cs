using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerDirectionMovement : MonoBehaviour
{

    ControllerAccessors controller;
    ConstantDirectionMover room;
	public float maxSpeed = 3f;
    // Use this for initialization
    void Start()
    {
        controller = GetComponent<ControllerAccessors>();
        room = GetComponentInParent<ConstantDirectionMover>();
    }

    // Update is called once per frame
    void Update()
    {
        if( controller.IsUnSqueezed() )
        {
            Vector3 v = controller.Velocity();
            room.SetDirection( v.normalized, v.magnitude.MapClamp( 0, 4, 0, maxSpeed ) );
        }

    }
}
