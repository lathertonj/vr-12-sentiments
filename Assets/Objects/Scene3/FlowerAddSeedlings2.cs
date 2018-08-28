using System.Collections;
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

    private void Update()
    {
        if( myController.IsFirstSqueezed() )
        {
            myChuck.BroadcastEvent( mySqueezedEvent );
        }

        if( myController.IsUnSqueezed() )
        {
            myChuck.BroadcastEvent( myUnsqueezedEvent );
            ReleaseSeedlings(); 
        }

        if( !myController.IsSqueezed() )
        {
            // never buzz when unsqueezed -- failsafe
            shouldBuzz = false;
        }

        if( shouldBuzz )
        {
            // TODO strength
            myController.Vibrate( 1000 );
        }

    }

    private void CreateSeedling()
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

        addedSeedlings.Add( newSeed );
    }

    private void ReleaseSeedlings()
    {
        mySonifier.PlayArpeggio( addedSeedlings.Count );
        myCollider.enabled = false;
        Invoke( "EnableCollider", 0.5f );

        foreach( Transform seedling in addedSeedlings )
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

        addedSeedlings = new List<Transform>();
    }

    private void EnableCollider()
    {
        myCollider.enabled = true;
    }
}
