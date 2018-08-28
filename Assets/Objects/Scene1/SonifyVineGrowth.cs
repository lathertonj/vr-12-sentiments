using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonifyVineGrowth : MonoBehaviour
{
    public double[] chord1, chord2, chord3, chord4;
    public double bass1, bass2, bass3, bass4;
    private bool inSecondHalf = false;
    private float timeOfLastChordSwitch = -1;
    
    // Use this for initialization
    void Start()
    {
        TheChuck.instance.RunCode( @"
            [62.0, 66, 71] @=> global float scene1VineChord[];
            47 => global float scene1VineBass;
            false => int shouldBePlaying;
            global Event shouldStartPlaying, shouldStopPlaying;

            ModalBar modey => JCRev r => dac;

            // set the gain
            .9 => r.gain;
            // set the reverb mix
            .05 => r.mix;

            0.15::second => dur noteLength;
            true => int hardPick;

            fun void PlayArray()
            {
                2 => modey.preset; // I like 6 and 2
                while( true ) 
                {
                    for( int i; i < scene1VineChord.size(); i++ )
                    {
                        if( shouldBePlaying && scene1VineChord[i] > 10 )
                        {
                            // strike position
                            Math.random2f( 0.2, 0.8 ) => modey.strikePosition;
                            // freq
                            scene1VineChord[i] => Std.mtof => modey.freq;
                            // strike it!
                            Math.random2f( 0.3, 0.4 ) + 0.17 * hardPick => modey.strike;
                            // next pick in opposite direction
                            !hardPick => hardPick;
                        }
                        else
                        {
                            // next pick in stronger direction
                            true => hardPick;
                        }
        
                        noteLength => now;
        
                        // turn off? this isn't it
                        1 => modey.damp;

                        // with small probability, rest
                        if( Math.randomf() < 0.05 )
                        {
                            noteLength => now;
                        }
                    }
                }
            }
            spork ~ PlayArray();
            
            while( true )
            {
                shouldStartPlaying => now;
                true => shouldBePlaying;
                shouldStopPlaying => now;
                false => shouldBePlaying;
            }
        " );    
    }

    private bool nextChordIsFirst = false;
    public void StartPlaying()
    {
        if( Time.time - timeOfLastChordSwitch > 1 )
        {
            // debounce chord switches to at most 1 per second
            nextChordIsFirst = !nextChordIsFirst;
            timeOfLastChordSwitch = Time.time;
        }
        
        // pick what chord
        double[] chord;
        if( inSecondHalf )
        {
            chord = nextChordIsFirst ? chord3 : chord4;
        }
        else
        {
            chord = nextChordIsFirst ? chord1 : chord2;
        }

        // tell chuck the chord
        TheChuck.instance.SetFloatArray( "scene1VineChord", chord );

        // tell chuck to start playing
        TheChuck.instance.BroadcastEvent( "shouldStartPlaying" );
    }

    public void StopPlaying()
    {
        // tell chuck to stop playing
        TheChuck.instance.BroadcastEvent( "shouldStopPlaying" );
    }
}
