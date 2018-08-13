using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonifySunbeams : MonoBehaviour
{
    private ChuckSubInstance myChuck;

    // Use this for initialization
    void Start()
    {
        myChuck = GetComponent<ChuckSubInstance>();

        myChuck.RunCode(@"
            ModalBar modey => JCRev r => dac;

            // set the gain
            .9 => r.gain;
            // set the reverb mix
            .05 => r.mix;

            0.12::second => dur noteLength;
            true => int hardPick;

            69 => int A4;
            71 => int B4;
            74 => int D5;
            76 => int E5;
            78 => int Fs5;
            79 => int G5;
            81 => int A5;
            83 => int B5;
            86 => int D6;

            0.3 => float offset;
            0 => global float sunbeamOffset;
            [B5+0.0, B5+1, B5+2, B5+3, B5+2, B5+1, B5] @=> float notes[];
            B5 + offset => Std.mtof => modey.freq;

            fun void PlayArrayInterp( float notes[], int numInterpNotes )
            {
                5 => modey.preset; // I like 3 and 5
                for( int i; i < notes.size() - 1; i++ )
                {
                    if( notes[i] > 10 )
                    {
                        // compute
                        notes[i+1] + offset + 1.05 * sunbeamOffset => float nextNote;
                        modey.freq() => Std.ftom => float currentNote;
                        ( nextNote - currentNote ) * 1.0 / numInterpNotes => float interpAmount;
                        for( int j; j < numInterpNotes; j++ )
                        {

                            // strike position
                            Math.random2f( 0.2, 0.8 ) => modey.strikePosition;
                            // freq
                            currentNote + j * interpAmount => Std.mtof => modey.freq;
                            // strike it!
                            Math.random2f( 0.3, 0.4 ) + 0.17 * hardPick => modey.strike;
                            // next pick in opposite direction
                            !hardPick => hardPick;
                            // wait
                            noteLength => now;
                        }
                    }
                    else
                    {
                        // next pick in stronger direction
                        true => hardPick;
                        numInterpNotes::noteLength => now;
                    }
        
        
        
                    // turn off? this isn't it
                    1 => modey.damp;
                }
    

            }


            // our main loop

            while( true )
            {
                PlayArrayInterp( notes, 12 );
            }
        ");
    }

    // Update is called once per frame
    void Update()
    {
        // clamp sunbeam to 20 and send to chuck
        myChuck.SetFloat( "sunbeamOffset", Mathf.Clamp( SunbeamInteractors.sunbeamAccumulated, 0, 20 ) );
    }
}
