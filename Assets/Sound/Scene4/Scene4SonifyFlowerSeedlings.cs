using System.Collections;
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
                while( true )
                {{
                    for( int i; i < {1}.size(); i++ )
                    {{
                        if( {1}[i] > 10 )
                        {{
                            Math.random2f( 0.2, 0.8 ) => {0}.strikePosition;
                            {1}[i] => Std.mtof => {0}.freq;

                            me.yield();

                            Math.random2f( 0.3, 0.4 ) + playStrong * 0.17 => {0}.strike;
                            !playStrong => playStrong;
                            jumpDelay => now;
                            {5}.broadcast();
                        }}
                        noteLength - jumpDelay => now;
                    }}
                }}
            }}

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
        Debug.Log( "playing " + numNotes.ToString() );
        // strategy 1: just fill the array, repeating
        /* int j = 0;
        double[] newArpeggio = new double[numNotes];
        for( int i = 0; i < numNotes; i++ )
        {
            newArpeggio[i] = myArpeggio[j];
            j++;
            j %= myArpeggio.Length;
        }*/

        // strategy 2: fill it with one copy, then random notes
        string[] newArpeggio = new string[numNotes];
        float[] toReturn = new float[numNotes];
        int j = 0;
        int prevJ = j;
        for( int i = 0; i < numNotes; i++ )
        {
            if( i < myArpeggio.Length )
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

        myChuck.RunCode( string.Format( @"
            {1} @=> float {0}[]; // global float {0}[4];
                                 // 2::ms => now;

            global ModalBar {2};

            0.05::second => dur noteLength;
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
        // myChuck.SetFloatArray( myArpeggioNotesVar, newArpeggio );

        return toReturn;
    }

    public void SonifySpewingNotes( float waitTime, float noteLength )
    {
        string[] newArpeggio = new string[myArpeggio.Length];
        for( int i = 0; i < newArpeggio.Length; i++ )
        {
            newArpeggio[i] = myArpeggio[i].ToString("0.0");
        }

        string notes = "[" + string.Join( ", ", newArpeggio ) + "]";
        myChuck.RunCode( string.Format( @"
            {1} @=> float {0}[]; // global float {0}[4];
                                 // 2::ms => now;

            global ModalBar {2};

            {4}::second => dur noteLength;
            true => int hardPick;

            // wait time
            {3}::second => now;
            
            fun void PlayArrayRandomly()
            {{
                int i, prevI;
                while( true )
                {{
                    // play note
                    // strike position
                    Math.random2f( 0.2, 0.8 ) => {2}.strikePosition;
                    // freq
                    {0}[i] => Std.mtof => {2}.freq;
                    // strike it!
                    Math.random2f( 0.3, 0.4 ) + 0.17 * hardPick => {2}.strike;
                    // next pick in opposite direction
                    !hardPick => hardPick;
                    

                    // choose next note
                    while( i == prevI )
                    {{
                        Math.random2( 0, {0}.size() - 1 ) => i;
                    }}
                    i => prevI;

                    noteLength => now;
                }}
            }}
            PlayArrayRandomly();
            
        ", myArpeggioNotesVar, notes, myModey, waitTime, noteLength ) );
    }
}
