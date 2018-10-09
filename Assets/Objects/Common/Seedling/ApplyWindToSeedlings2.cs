using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyWindToSeedlings2 : MonoBehaviour
{
    Rigidbody[] seedlings;
    Vector3 currentWindDirection;
    public float maxWindIntensity = 1f;
    public float maxAngleChange = 120;

    public ChuckSubInstance myChuck;
    private ChuckFloatSyncer myIntensitySyncer;

    // Use this for initialization
    void Start()
    {
        ChuckEventListener myChangeDirectionListener = gameObject.AddComponent<ChuckEventListener>();
        myChangeDirectionListener.ListenForEvent( myChuck, "scene6SwellStart", ChangeWindDirection );

        myIntensitySyncer = gameObject.AddComponent<ChuckFloatSyncer>();
        myIntensitySyncer.SyncFloat( myChuck, "scene6SwellIntensity" );

        seedlings = GetComponentsInChildren<Rigidbody>();
        currentWindDirection = Vector3.right;
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
        float currentWindIntensity = maxWindIntensity * 0.0005f * myIntensitySyncer.GetCurrentValue();

        foreach( Rigidbody seedling in seedlings )
        {
            seedling.AddForce( currentWindDirection * currentWindIntensity );
        }
    }
}
