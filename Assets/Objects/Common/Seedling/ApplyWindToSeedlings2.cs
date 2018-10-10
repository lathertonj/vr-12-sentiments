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
    private ChuckEventListener mySecondHalfListener;

    // Use this for initialization
    void Start()
    {
        ChuckEventListener myChangeDirectionListener = gameObject.AddComponent<ChuckEventListener>();
        myChangeDirectionListener.ListenForEvent( myChuck, "scene6SwellStart", ChangeWindDirection );

        myIntensitySyncer = gameObject.AddComponent<ChuckFloatSyncer>();
        myIntensitySyncer.SyncFloat( myChuck, "scene6SwellIntensity" );

        mySecondHalfListener = gameObject.AddComponent<ChuckEventListener>();
        mySecondHalfListener.ListenForEvent( myChuck, "ahhChordChange", AdvanceToSecondHalf );

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

    private bool inSecondHalf = false;
    private void AdvanceToSecondHalf()
    {
        inSecondHalf = true;
        // only listen to the first one
        mySecondHalfListener.StopListening();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float currentWindIntensity = maxWindIntensity * 0.0005f * myIntensitySyncer.GetCurrentValue();
        
        Vector3 forceDirection = Vector3.zero;
        Vector3 forceTorque = Vector3.zero;
        // in second half, mostly go up; otherwise, go sideways
        if( inSecondHalf )
        {
            forceDirection = currentWindIntensity * Random.Range( 0.8f, 1.8f ) * Vector3.up;
            forceDirection += currentWindDirection * 0.01f * currentWindIntensity;
            forceTorque = new Vector3(
                Random.Range( -1f, 1f ),
                Random.Range( -1f, 1f ),
                Random.Range( -1f, 1f )
            );
        }
        else
        {
            forceDirection = currentWindDirection * currentWindIntensity;
        }

        // apply the force
        foreach( Rigidbody seedling in seedlings )
        {
            seedling.AddForce( forceDirection );
            //seedling.AddTorque( forceTorque );
        }
    }
}
