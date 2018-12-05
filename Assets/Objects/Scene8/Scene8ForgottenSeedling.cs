using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene8ForgottenSeedling : MonoBehaviour
{
	private Rigidbody myRB;
	private Vector3 currentPos, goalPos;
	private float posSlew;

    // Use this for initialization
    void Start()
    {
		myRB = GetComponent<Rigidbody>();
		myRB.useGravity = false;
		myRB.isKinematic = true;
		currentPos = transform.position;
		goalPos = currentPos - Random.Range( 0.115f, 0.13f ) * Vector3.up;
		posSlew = 1; // * deltaTime
    }

    // Update is called once per frame
    void Update()
    {
		currentPos += posSlew * Time.deltaTime * ( goalPos - currentPos );
		transform.position = currentPos;
		if( ( currentPos - goalPos ).magnitude < 0.001f )
		{
			Destroy( this );
		}
    }
}
