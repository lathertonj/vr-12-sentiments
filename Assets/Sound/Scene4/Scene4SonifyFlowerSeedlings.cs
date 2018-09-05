﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( ChuckSubInstance ) )]
public class Scene4SonifyFlowerSeedlings : MonoBehaviour
{
    private ChuckSubInstance myChuck;
    public double[] myChord;
    public double[] myArpeggio;

    private string myChordNotesVar, myArpeggioNotesVar, myModey;
    private string mySqueezedEvent, myUnsqueezedEvent;
    private string myJumpEvent;
    public void StartChuck( float jumpDelay, System.Action launchASeedling )
    {
        myChuck = GetComponent<ChuckSubInstance>();
        myChordNotesVar = myChuck.GetUniqueVariableName();
        myArpeggioNotesVar = myChuck.GetUniqueVariableName();
        myModey = myChuck.GetUniqueVariableName();
        myJumpEvent = myChuck.GetUniqueVariableName();
        mySqueezedEvent = myChuck.GetUniqueVariableName();
        myUnsqueezedEvent = myChuck.GetUniqueVariableName();


        // respond to individual notes
        myChuck.RunCode( string.Format( @"
            global ModalBar {0} => JCRev reverb => dac;
            global float {1}[1];
            global Event {2}, {3}, {5};
            {4}::second => dur noteLength;
            0.08::second => dur jumpDelay;

            true => int playStrong;

            fun void PlayNotes()
            {{
                int i, prevI;
                while( true )
                {{
                    // select next note index
                    while( i == prevI && {1}.size() > 1 )
                    {{
                        Math.random2( 0, {1}.size() - 1 ) => i;
                        me.yield();
                    }}
                    i => prevI;

                    // play note
                    if( {1}[i] > 10 )
                    {{
                        Math.random2f( 0.2, 0.8 ) => {0}.strikePosition;
                        {1}[i] => Std.mtof => {0}.freq;

                        me.yield();

                        Math.random2f( 0.3, 0.4 ) + playStrong * 0.17 => {0}.strike;
                        !playStrong => playStrong;
                        jumpDelay => now;
                        
                        // signal a jump should happen
                        {5}.broadcast();
                    }}
                    else
                    {{
                        // only wait
                        jumpDelay => now; 
                    }}

                    // wait for next note
                    noteLength - jumpDelay => now;
                }}
            }}

            fun void IncreaseNoteSpeed()
            {{
                jumpDelay + 0.03::second => dur minSpeed;
                // actually let's make it longer min speed
                0.25::second => minSpeed;
                while( true )
                {{
                    // unsqueezed
                    {3} => now;

                    // --> make the jumps fast
                    0.90 *=> noteLength;
                    if( noteLength < minSpeed )
                    {{
                        minSpeed => noteLength;
                        // TODO: send a signal outward?
                        return;
                    }}
                }}
            }}
            spork ~ IncreaseNoteSpeed();

            while( true )
            {{
                spork ~ PlayNotes() @=> Shred playNotesShred;
                {2} => now; // when squeezed, stop jumping and playing
                playNotesShred.exit();
                {3} => now;
                5::noteLength => now; // wait 3 notes after unsqueezed to resume
            }}

        ", myModey, myChordNotesVar, mySqueezedEvent, myUnsqueezedEvent, jumpDelay, myJumpEvent ) );
        myChuck.SetFloatArray( myChordNotesVar, myChord );
        gameObject.AddComponent<ChuckEventListener>().ListenForEvent( myChuck, myJumpEvent, launchASeedling );
    }

    public void InformSqueezed()
    {
        myChuck.BroadcastEvent( mySqueezedEvent );
    }

    public void InformUnsqueezed()
    {
        myChuck.BroadcastEvent( myUnsqueezedEvent );
    }

    public float[] PlayArpeggio( int numNotes )
    {
        // fill array with PART OF one copy, then random notes
        string[] newArpeggio = new string[numNotes];
        float[] toReturn = new float[numNotes];
        int j = 0;
        int prevJ = j;
        for( int i = 0; i < numNotes; i++ )
        {
            if( i < myArpeggio.Length / 3 ) // only take first third of array
            {
                j = i;
            }
            else
            {
                while( j == prevJ )
                {
                    j = Random.Range( 0, myArpeggio.Length );
                }
            }
            toReturn[i] = (float) myArpeggio[j];
            newArpeggio[i] = myArpeggio[j].ToString("0.0");
            prevJ = j;
        }

        string notes = "[" + string.Join( ", ", newArpeggio ) + "]";
        Debug.Log( notes );

        myChuck.RunCode( string.Format( @"
            {1} @=> float {0}[]; 

            global ModalBar {2};

            0.10::second => dur noteLength;
            true => int hardPick;

            fun void PlayArray()
            {{
                for( int i; i < {0}.size(); i++ )
                {{
                    if( {0}[i] > 10 )
                    {{
                        // strike position
                        Math.random2f( 0.2, 0.8 ) => {2}.strikePosition;
                        // freq
                        {0}[i] => Std.mtof => {2}.freq;
                        // strike it!
                        Math.random2f( 0.3, 0.4 ) + 0.17 * hardPick => {2}.strike;
                        // next pick in opposite direction
                        !hardPick => hardPick;
                    }}
                    else
                    {{
                        // next pick in stronger direction
                        true => hardPick;
                    }}
        
                    noteLength => now;
                    1.09 *=> noteLength;
                }}
            }}
            PlayArray();
            5::second => now;
        ", myArpeggioNotesVar, notes, myModey ) );

        return toReturn;
    }
}
