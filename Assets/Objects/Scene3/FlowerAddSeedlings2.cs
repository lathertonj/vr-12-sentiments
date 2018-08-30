﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerAddSeedlings2 : MonoBehaviour
{
    public ControllerAccessors myController;
    public Transform seedlingPrefab;
    private SonifyFlowerSeedlings mySonifier;
    private ChuckSubInstance myChuck;
    private List<Transform> addedSeedlings;
    public Collider myCollider;

    public float numLongPresses = 0;

    // Use this for initialization
    void Start()
    {
        StartChuck();
        addedSeedlings = new List<Transform>();
    }

    private string myStartBuzzEvent, myStopBuzzEvent, mySqueezedEvent, myUnsqueezedEvent;
    private string myStartChordEvent, myStopChordEvent;
    private string myTatumFloat;
    private void StartChuck()
    {
        myChuck = GetComponent<ChuckSubInstance>();
        myStartBuzzEvent = myChuck.GetUniqueVariableName();
        myStopBuzzEvent = myChuck.GetUniqueVariableName();
        mySqueezedEvent = myChuck.GetUniqueVariableName();
        myUnsqueezedEvent = myChuck.GetUniqueVariableName();
        myStartChordEvent = myChuck.GetUniqueVariableName();
        myStopChordEvent = myChuck.GetUniqueVariableName();
        myTatumFloat = myChuck.GetUniqueVariableName();
        myChuck.RunCode( string.Format( @"
            global Event {0}, {1}, {2}, {3}, {4}, {5};
            // 0: start buzz (signal out)
            // 1: stop buzz  (signal out)
            // 2: controller squeezed   (signal in)
            // 3: controller unsqueezed (signal in)
            // 4: start chord (signal out)
            // 5: stop chord  (signal out)

            global float {6};

            fun void SendBuzzes()
            {{
                0.5::second => dur interBuzzTime;
                0.07::second => dur chordBeforeBuzzTime;
                while( true )
                {{
                    // tatum = 1/4 of current cycle time
                    ( interBuzzTime * 2 + chordBeforeBuzzTime ) / 4::second => {6};

                    // wait the chord before buzz time between chord start and buzz start
                    {4}.broadcast();
                    chordBeforeBuzzTime => now;
                    {0}.broadcast();

                    // wait the length of the buzz
                    interBuzzTime => now;

                    // stop the chord and the buzz at the same time
                    {5}.broadcast();
                    {1}.broadcast();

                    // wait the length of the buzz as a rest
                    interBuzzTime => now;
                    
                    // listen, if I've been told to exit
                    me.yield();

                    // speed up gradually
                    0.05::second -=> interBuzzTime;
                    if( interBuzzTime < 0.15::second ) {{ 0.15::second => interBuzzTime; }}
                }}
            }}

            while( true )
            {{
                {2} => now;
                spork ~ SendBuzzes() @=> Shred buzzShred;
                {3} => now;
                buzzShred.exit();
                {5}.broadcast(); // stop playing chord if unsqueezed
                {1}.broadcast(); // stop vibrating if unsqueezed
            }}

        ", myStartBuzzEvent, myStopBuzzEvent, 
           mySqueezedEvent, myUnsqueezedEvent, 
           myStartChordEvent, myStopChordEvent, 
           myTatumFloat ) );

        gameObject.AddComponent<ChuckEventListener>().ListenForEvent( myChuck, myStartBuzzEvent, StartBuzzing );
        gameObject.AddComponent<ChuckEventListener>().ListenForEvent( myChuck, myStopBuzzEvent, StopBuzzing );
        
        // tell sonifier which events to listen to for starting the chords
        // this all happens within chuck so no need to worry about unity timing
        mySonifier = GetComponent<SonifyFlowerSeedlings>();
        mySonifier.StartChuck( myStartChordEvent, myStopChordEvent, myTatumFloat, mySqueezedEvent, myUnsqueezedEvent );
    }

    bool shouldBuzz = false;
    private void StartBuzzing()
    {
        shouldBuzz = true;
        CreateSeedling();
    }

    private void StopBuzzing()
    {
        shouldBuzz = false;
    }

    bool shouldRespondToControllers = true;
    private void Update()
    {
        if( shouldRespondToControllers && myController.IsFirstSqueezed() )
        {
            myChuck.BroadcastEvent( mySqueezedEvent );
        }

        if( shouldRespondToControllers && myController.IsUnSqueezed() )
        {
            myChuck.BroadcastEvent( myUnsqueezedEvent );
            ReleaseSeedlings(); 
        }

        if( !myController.IsSqueezed() )
        {
            // never buzz when unsqueezed -- failsafe
            shouldBuzz = false;
        }

        // buzz if regular buzzing or if I turned the controllers off
        if( shouldBuzz || !shouldRespondToControllers )
        {
            // TODO strength
            myController.Vibrate( 1000 );
        }

    }

    private Transform CreateSeedling( bool addToList = true )
    {
        Vector3 newPosition = new Vector3(
            Random.Range( -1f, 1f ),
            0,
            Random.Range( -1f, 1f )
        );
        Quaternion newRotation = Quaternion.AngleAxis( Random.Range( 0, 359 ), Vector3.up );
        Transform newSeed = Instantiate( seedlingPrefab, /*newPosition + transform.position*/ transform.TransformPoint( newPosition * 0.005f ), transform.rotation, transform );
        newSeed.localRotation = newRotation;
        // set height to 0. why isn't it already?
        Vector3 p = newSeed.localPosition;
        p.y = 0;
        newSeed.localPosition = p;

        if( addToList )
        {
            addedSeedlings.Add( newSeed );
        }

        return newSeed;
    }

    private void ReleaseSeedlings()
    {
        if( addedSeedlings.Count > 11 )
        {
            numLongPresses++;
        }

        float[] notes = mySonifier.PlayArpeggio( addedSeedlings.Count );
        myCollider.enabled = false;
        Invoke( "EnableCollider", 0.5f );

        foreach( Transform seedling in addedSeedlings )
        {
            ReleaseOneSeedling( seedling );
        }

        addedSeedlings = new List<Transform>();
    }

    private void ReleaseOneSeedling( Transform seedling )
    {
        Rigidbody rb = seedling.GetComponent<Rigidbody>();
        // remove freezing of rotation or position
        rb.constraints = RigidbodyConstraints.None;
        // unparent
        seedling.parent = null;
        // set velocity / angular velocity
        rb.velocity = myController.Velocity();
        rb.angularVelocity = myController.AngularVelocity();
        // add small upward force in the direction of flower head
        rb.AddForce( 1 * transform.up, ForceMode.Impulse );
    }

    private void EnableCollider()
    {
        myCollider.enabled = true;
    }

    public void StartSpewing( float timeToStart, float period )
    {
        // This causes too much memory usage
        // InvokeRepeating( "SpewASeedling", timeToStart, period );

        // start my particle emitter which spews fake seedlings
        GetComponentInChildren<ParticleSystem>().Play();
        // play a lot of notes. ssshhh they aren't synced up
        mySonifier.SonifySpewingNotes( timeToStart, period );
        
        // start a new chord and don't stop 
        shouldRespondToControllers = false;
        mySonifier.AdvanceToSecondHalf();
        // sometimes it doesn't react to this and I don't understand why
        // TODO is it getting a stop chord event right afterward?
        myChuck.BroadcastEvent( myStartChordEvent );
        // do it again shortly in case of an out of order event error
        Invoke( "StartChordLate", 0.25f );


    }

    private void StartChordLate()
    {
        myChuck.BroadcastEvent( myStartChordEvent );
    }

    private void SpewASeedling()
    {
        ReleaseOneSeedling( CreateSeedling( addToList: false ) );
        mySonifier.SonifyRandomNote();
    }

    // I tried playing their note when you collide with them but it happens way
    // too much and sounds glitchy and awful
    /*private void OnCollisionEnter( Collision collision )
    {
        NumberHolder maybeSeedling = collision.gameObject.GetComponent<NumberHolder>();
        if( maybeSeedling != null )
        {
            mySonifier.SonifyIndividualNote( maybeSeedling.theNumber );
        }
    }*/
}
