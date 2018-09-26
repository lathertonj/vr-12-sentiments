using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyRandomRotationToSeedling : MonoBehaviour
{
    public float howMuch = 2f;
    public float howOften = 5f;
    Rigidbody rb;
    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        RotateIt();
        InvokeRepeating( "RotateIt", Random.Range( 1f, howOften - 1f ), howOften );
    }

    // Update is called once per frame
    void RotateIt()
    {
        rb.AddTorque( howMuch * new Vector3(
            Random.Range( -1f, 1f ),
            Random.Range( -1f, 1f ),
            Random.Range( -1f, 1f )
        ), ForceMode.VelocityChange );
    }
}
