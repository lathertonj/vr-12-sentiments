using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyWindToSeedlings : MonoBehaviour
{
    Rigidbody[] seedlings;
    Vector3 currentWindDirection;
    float windIntensityCycle = 5; // seconds
    public float maxWindIntensity = 1f;

    // Use this for initialization
    void Start()
    {
        seedlings = GetComponentsInChildren<Rigidbody>();
        currentWindDirection = Vector3.right;
        InvokeRepeating( "ChangeWindDirection", windIntensityCycle, windIntensityCycle );
    }

    void ChangeWindDirection()
    {
        // rotate by somewhere between -120 and 120 degrees
        currentWindDirection = Quaternion.AngleAxis( Random.Range( -120, 120 ), Vector3.up ) * currentWindDirection;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float currentWindIntensity = maxWindIntensity * 0.0005f * Mathf.Pow( 
            Mathf.Sin( Mathf.PI * Time.fixedTime / windIntensityCycle ),
            2 
        );

        foreach( Rigidbody seedling in seedlings )
        {
            seedling.AddForce( currentWindDirection * currentWindIntensity );
        }
    }
}
