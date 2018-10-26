﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene7SonifyFlowerSeedlings : MonoBehaviour {

	private ChuckSubInstance myChuck;
    public double[] myChord;

    private string myJumpEvent, myIncreaseSpeedEvent, myDecreaseSpeedEvent;
    public void StartChuck( float jumpDelay, System.Action launchASeedling, int numSeedlings )
    {
        myChuck = GetComponent<ChuckSubInstance>();
        myJumpEvent = myChuck.GetUniqueVariableName();
        myIncreaseSpeedEvent = myChuck.GetUniqueVariableName();
        myDecreaseSpeedEvent = myChuck.GetUniqueVariableName();

        // copy my chord into longer version that just repeats it
        string[] expandedChord = new string[numSeedlings];
        int j = 0;
        for( int i = 0; i < expandedChord.Length; i++ )
        {
            expandedChord[i] = myChord[j].ToString( "0.00" );
            j++;
            j %= myChord.Length;
        }

        string notesString = "[" + string.Join( ", ", expandedChord ) + "]";

        // respond to individual notes
        myChuck.RunCode( string.Format( @"
            ModalBar modey => JCRev reverb => dac;
            {2} @=> float notes[];
            global Event {1}, {3}, {4};
            {0}::second => dur noteLength;
            0.29::second => dur happyModeCutoff;
            global int scene7HappyMode;
            0.05::second => dur jumpDelay;

            true => int playStrong;

            fun void PlayNotes()
            {{
                while( true )
                {{
                    for( int i; i < notes.size(); i++ )
                    {{
                        // play note
                        if( notes[i] > 10 )
                        {{
                            Math.random2f( 0.2, 0.8 ) => modey.strikePosition;
                            if( scene7HappyMode && maybe )
                            {{
                                notes[i] + 12 => Std.mtof => modey.freq;
                            }}
                            else
                            {{
                                notes[i] => Std.mtof => modey.freq;
                            }}

                            me.yield();

                            Math.random2f( 0.3, 0.4 ) + playStrong * 0.17 => modey.strike;
                            !playStrong => playStrong;
                            jumpDelay => now;
                        
                            // signal a jump should happen
                            {1}.broadcast();
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
            }}

            fun void IncreaseNoteSpeed()
            {{
                // min speed
                0.15::second => dur minSpeed;
                while( true )
                {{
                    // time to increase
					{3} => now;

                    // --> make the jumps fast
                    0.94 *=> noteLength;
                    if( noteLength < minSpeed )
                    {{
                        minSpeed => noteLength;
                    }}

                    if( noteLength < happyModeCutoff )
                    {{
                        true => scene7HappyMode;
                    }}
                }}
            }}
            spork ~ IncreaseNoteSpeed();

            fun void DecreaseNoteSpeed()
            {{
                // min speed
                0.5::second => dur maxSpeed;
                while( true )
                {{
                    // time to decrease
					{4} => now;

                    // --> make the jumps slow
                    1.02 *=> noteLength;
                    if( noteLength > maxSpeed )
                    {{
                        maxSpeed => noteLength;
                    }}

                    // exit happy mode on any slowdown
                    false => scene7HappyMode;
                }}
            }}
            spork ~ DecreaseNoteSpeed();

            spork ~ PlayNotes() @=> Shred playNotesShred;


            while( true ) {{ 1::second => now; }}
            playNotesShred.exit();
            

        ", jumpDelay, myJumpEvent, notesString, myIncreaseSpeedEvent, myDecreaseSpeedEvent ) );
        gameObject.AddComponent<ChuckEventListener>().ListenForEvent( myChuck, myJumpEvent, launchASeedling );
    }

    public void FastMovementHappened()
    {
        myChuck.BroadcastEvent( myIncreaseSpeedEvent );
    }

    public void SlowMovementHappened()
    {
        myChuck.BroadcastEvent( myDecreaseSpeedEvent );
    }
}
