using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene7SonifyFlowerSeedlings : MonoBehaviour {

	private ChuckSubInstance myChuck;
    public double[] myChord;

    private string myJumpEvent;
    public void StartChuck( float jumpDelay, System.Action launchASeedling, int numSeedlings )
    {
        myChuck = GetComponent<ChuckSubInstance>();
        myJumpEvent = myChuck.GetUniqueVariableName();

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
            global Event {1};
            {0}::second => dur noteLength;
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
                            notes[i] => Std.mtof => modey.freq;

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
                    // unsqueezed
					// TODO what event causes increases in speed?
					1::week => now;
                    //  => now;

                    // --> make the jumps slow
                    0.98 *=> noteLength;
                    if( noteLength < minSpeed )
                    {{
                        minSpeed => noteLength;
                        // TODO: send a signal outward?
                        return;
                    }}
                }}
            }}
            spork ~ IncreaseNoteSpeed();

            spork ~ PlayNotes() @=> Shred playNotesShred;


            while( true ) {{ 1::second => now; }}
            playNotesShred.exit();
            

        ", jumpDelay, myJumpEvent, notesString ) );
        gameObject.AddComponent<ChuckEventListener>().ListenForEvent( myChuck, myJumpEvent, launchASeedling );
    }
}
