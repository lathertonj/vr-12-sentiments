using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantDirectionMoverHeadColliderCheck : MonoBehaviour
{
	ConstantDirectionMover room;
    // Use this for initialization
    void Start()
    {
		room = GetComponentInParent<ConstantDirectionMover>();
    }

    void OnTriggerEnter( Collider other )
    {
		// if we hit the ground, stop moving.
        if( other.gameObject.CompareTag( "Ground" ) )
        {
			room.SetDirection( Vector3.zero, 0 );
        }
    }
}
