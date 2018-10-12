using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyWindToSeedlings2 : MonoBehaviour
{
    private static bool inEnding = false;
    public static void DoEnding()
    {
        inEnding = true;
    }

    Rigidbody[] seedlings;
    float[] maxSeedlingHeights;
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
        myChangeDirectionListener.ListenForEvent( myChuck, "scene6SwellStart", RespondToSwell );

        myIntensitySyncer = gameObject.AddComponent<ChuckFloatSyncer>();
        myIntensitySyncer.SyncFloat( myChuck, "scene6SwellIntensity" );

        mySecondHalfListener = gameObject.AddComponent<ChuckEventListener>();
        mySecondHalfListener.ListenForEvent( myChuck, "ahhChordChange", StartSecondHalfSwell );

        ChuckEventListener mySecondHalfListener2 = gameObject.AddComponent<ChuckEventListener>();
        mySecondHalfListener2.ListenForEvent( myChuck, "ahhChordFadeOut", EndSecondHalfSustain );

        seedlings = GetComponentsInChildren<Rigidbody>();
        maxSeedlingHeights = new float[seedlings.Length];
        PickNewMaxHeights();
        currentWindDirection = Vector3.right;
    }

    void RespondToSwell()
    {
        // change wind direction
        // rotate by somewhere between -120 and 120 degrees
        currentWindDirection = Quaternion.AngleAxis(
            Random.Range( -maxAngleChange, maxAngleChange ), Vector3.up
        ) * currentWindDirection;
    }

    private bool inSecondHalf = false;
    private float numSecondHalfSwells = 0;
    private void StartSecondHalfSwell()
    {
        inSecondHalf = true;
        PickNewMaxHeights();
        // turn off gravity
        SetGravity( false );
        currentRotationSpeed = numSecondHalfSwells.MapClamp( 0, 16, minRotationSpeed, maxRotationSpeed );
        currentUpForce = numSecondHalfSwells.MapClamp( 0, 16, minUpForce, maxUpForce );

        numSecondHalfSwells++;
    }

    private void EndSecondHalfSustain()
    {
        // only set gravity back down if we are not in the ending
        SetGravity( !inEnding );
    }

    private void PickNewMaxHeights()
    {
        for( int i = 0; i < maxSeedlingHeights.Length; i++ )
        {
            maxSeedlingHeights[i] = Random.Range( 0.15f, 1f ) * currentUpForce;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float currentWindIntensity = maxWindIntensity * 0.0005f * myIntensitySyncer.GetCurrentValue();

        Vector3 forceDirection = Vector3.zero;
        if( !inSecondHalf )
        {
            forceDirection = currentWindDirection * currentWindIntensity;
        }

        // apply the force
        for( int i = 0; i < seedlings.Length; i++ )
        {
            Rigidbody seedling = seedlings[i];
            if( inSecondHalf )
            {
                forceDirection = Vector3.up * currentWindIntensity * maxSeedlingHeights[i];
            }
            seedling.AddForce( forceDirection );
        }
    }

    public float minRotationSpeed = 30, maxRotationSpeed = 120;
    private float currentRotationSpeed;
    public float minUpForce = 0.01f, maxUpForce = 0.05f;
    private float currentUpForce;
    void Update()
    {
        float currentWindIntensity = myIntensitySyncer.GetCurrentValue();
        if( inSecondHalf )
        {
            //transform.Rotate( Vector3.up, rotationSpeed * currentWindIntensity * Time.deltaTime );
            for( int i = 0; i < seedlings.Length; i++ )
            {
                Rigidbody seedling = seedlings[i];

                // galaxy rotation (according to how far from center)
                // TODO make based on currentWindIntensity
                Vector3 rotationConsideration = seedling.transform.localPosition; rotationConsideration.y = 0;
                float rotationAmount = currentWindIntensity * rotationConsideration.magnitude.MapClamp( 0.1f, 1f, 1f, 0.3f );
                seedling.transform.parent.Rotate( Vector3.up, currentRotationSpeed * rotationAmount * Time.deltaTime );
            }
        }
    }

    private void SetGravity( bool g )
    {
        foreach( Rigidbody seedling in seedlings )
        {
            seedling.useGravity = g;
        }
    }
}
