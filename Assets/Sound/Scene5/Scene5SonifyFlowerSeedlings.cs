using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( ChuckSubInstance ) )]
public class Scene5SonifyFlowerSeedlings : MonoBehaviour
{
    private ChuckSubInstance myChuck;
    public double[] myChord;

    private string myJumpEvent;
    private string myMuteNoteNumber, myMuteNoteEvent;
    public void StartChuck( float jumpDelay, System.Action launchASeedling, int numSeedlings )
    {
        myChuck = GetComponent<ChuckSubInstance>();
        myJumpEvent = myChuck.GetUniqueVariableName();
        myMuteNoteEvent = myChuck.GetUniqueVariableName();
        myMuteNoteNumber = myChuck.GetUniqueVariableName();

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
            global int {3};
            global Event {1}, {4};
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

            fun void DecreaseNoteSpeed()
            {{
                // max speed
                0.5::second => dur maxSpeed;
                while( true )
                {{
                    // unsqueezed
                    {4} => now;

                    // --> make the jumps slow
                    1.05 *=> noteLength;
                    if( noteLength > maxSpeed )
                    {{
                        maxSpeed => noteLength;
                        // TODO: send a signal outward?
                        return;
                    }}
                }}
            }}
            spork ~ DecreaseNoteSpeed();

            fun void MuteNotes()
            {{
                while( true )
                {{
                    {4} => now;
                    0 => notes[{3}];
                }}
            }}
            spork ~ MuteNotes();

            spork ~ PlayNotes() @=> Shred playNotesShred;

            global Event scene5FadeSeedlings;
            scene5FadeSeedlings => now;

            // Actually, just let the whole shred exit
            playNotesShred.exit();
            while( true ) {{ 1::second => now; }}
            
            /*while( true )
            {{
                // x ^ 6000 = 0.001
                0.998849 * modey.gain() => modey.gain;
                1::ms => now;
            }}*/

        ", jumpDelay, myJumpEvent, notesString, myMuteNoteNumber, myMuteNoteEvent ) );
        gameObject.AddComponent<ChuckEventListener>().ListenForEvent( myChuck, myJumpEvent, launchASeedling );
    }

    public void InformLostSeedling( int seedlingNumber )
    {
        myChuck.SetInt( myMuteNoteNumber, seedlingNumber );
        myChuck.BroadcastEvent( myMuteNoteEvent );
    }
}
