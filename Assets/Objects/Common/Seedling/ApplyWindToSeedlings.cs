using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyWindToSeedlings : MonoBehaviour
{
    Rigidbody[] seedlings;
    Vector3 currentWindDirection;
    public float windIntensityCycle = 5; // seconds
    public float maxWindIntensity = 1f;
    public float maxAngleChange = 120;
    public bool applyRandomTorque = false;
    public bool randomizeInitialLocations = false;

    // Use this for initialization
    void Start()
    {
        seedlings = GetComponentsInChildren<Rigidbody>();
        currentWindDirection = Vector3.right;
        InvokeRepeating( "ChangeWindDirection", windIntensityCycle, windIntensityCycle );

        foreach( Rigidbody seedling in seedlings )
        {
            seedling.transform.localPosition = new Vector3(
                Random.Range( -0.75f, 0.75f ),
                Random.Range( -0.15f, 0.15f ),
                Random.Range( -0.75f, 0.75f )
            );
        }
    }

    void ChangeWindDirection()
    {
        // rotate by somewhere between -120 and 120 degrees
        currentWindDirection = Quaternion.AngleAxis(
            Random.Range( -maxAngleChange, maxAngleChange ), Vector3.up
        ) * currentWindDirection;
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
            if( applyRandomTorque && Random.Range( 0, 1 ) > 0.4f )
            {
                seedling.AddTorque( new Vector3( 
                    Random.Range( -currentWindIntensity, currentWindIntensity ),
                    Random.Range( -currentWindIntensity, currentWindIntensity ),
                    Random.Range( -currentWindIntensity, currentWindIntensity )
                ) );
            }
        }
    }
}
