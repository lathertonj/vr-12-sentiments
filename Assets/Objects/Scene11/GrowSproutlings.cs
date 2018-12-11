using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowSproutlings : MonoBehaviour
{
    private Transform[] mySproutlingGroups;
    private int currentIndex = -1;
    private float goalPos, posSlew;
    private bool shouldSlew = false;

    private ChuckSubInstance myChuck;

    // Use this for initialization
    void Start()
    {
        mySproutlingGroups = new Transform[transform.childCount];
        for( int i = 0; i < mySproutlingGroups.Length; i++ )
        {
            mySproutlingGroups[i] = transform.GetChild( i );
        }

        posSlew = 0.007f;

        myChuck = GetComponent<ChuckSubInstance>();
        myChuck.RunCode( @"
			global Event scene11GrowSproutlings, scene10ChordChange;
            fun void SendGrowSproutlingsEvent()
            {{
				// this script is being run as a result of this script broadcasting:
                // scene10AdvanceToScene11 => now;
                // in second half now!

                // after first chord change, we're on the second chord
                scene10ChordChange => now;

                // grow!
                scene11GrowSproutlings.broadcast();

                repeat( 2 ) 
                {{
                    // wait til second chord change again
                    repeat( 2 ) {{ scene10ChordChange => now; }}

                    // grow!
                    scene11GrowSproutlings.broadcast();
                }}
            }}
            SendGrowSproutlingsEvent();
		" );
        ChuckEventListener grow = gameObject.AddComponent<ChuckEventListener>();
        grow.ListenForEvent( myChuck, "scene11GrowSproutlings", RaiseNextGroup );
    }

    // Update is called once per frame
    void Update()
    {
        if( shouldSlew )
        {
            mySproutlingGroups[currentIndex].localPosition += posSlew * ( goalPos - mySproutlingGroups[currentIndex].localPosition.y ) * Vector3.up;

            if( Mathf.Abs( mySproutlingGroups[currentIndex].localPosition.y - goalPos ) < 0.001 )
            {
                shouldSlew = false;
            }
        }
    }

    void RaiseNextGroup()
    {
        currentIndex++;
        if( currentIndex >= mySproutlingGroups.Length ) return;

        mySproutlingGroups[currentIndex].gameObject.SetActive( true );
        goalPos = 0;
        shouldSlew = true;
    }
}
