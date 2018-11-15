using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene7SonifyFlowerSeedlings : MonoBehaviour {

	private ChuckSubInstance myChuck;
    public double[] myChord;

    private string myJumpEvent, myIncreaseSpeedEvent, myDecreaseSpeedEvent, myNextLoudness;
    public void StartChuck( float jumpDelay, System.Action launchASeedling, int numSeedlings )
    {
        myChuck = GetComponent<ChuckSubInstance>();
        myJumpEvent = myChuck.GetUniqueVariableName();
        myIncreaseSpeedEvent = myChuck.GetUniqueVariableName();
        myDecreaseSpeedEvent = myChuck.GetUniqueVariableName();
        myNextLoudness = myChuck.GetUniqueVariableName();

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
            false => global int scene7HappyFinishMode;
            0.05::second => dur jumpDelay;
            global float {5};

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

                            {5} => modey.gain;

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



            Shakers s => JCRev rev2 => dac;
            0.05 => rev2.mix;

            // params
            0.7 => s.decay; // 0 to 1
            50 => s.objects; // 0 to 128
            11 => s.preset; // 0 to 22.  0 is good for galloping. 
            // 11 is good for eighths

            fun void OnBeats()
            {{
                while( true )
                {{
                    if( scene7HappyMode || maybe )
                    {{
                        Math.random2f( 0.7, 0.9 ) => s.energy;
                        1 => s.noteOn;
                    }}
                    noteLength => now;
    
                    if( scene7HappyMode || maybe )
                    {{
                        Math.random2f( 0.3, 0.5 ) => s.energy;
                        1 => s.noteOn;
                    }}
                    noteLength => now;
                }}
            }}
            spork ~ OnBeats() @=> Shred playShakersShred;

            fun void ShakersGain()
            {{
                float currentGain;
                0.001 => float upSlew;
                0.0003 => float downSlew;
                while( true ) 
                {{
                    if( scene7HappyMode > currentGain )
                    {{
                        upSlew * ( scene7HappyMode - currentGain ) +=> currentGain;
                    }}
                    else
                    {{
                        downSlew * ( scene7HappyMode - currentGain ) +=> currentGain;
                    }}
                    currentGain * 0.3 => s.gain;
                    1::ms => now;
                }}
            }}
            spork ~ ShakersGain();

            fun void OverrideHappyMode()
            {{
                while( true )
                {{
                    if( scene7HappyFinishMode )
                    {{
                        true => scene7HappyMode;
                    }}
                    1::ms => now;
                }}
            }}
            spork ~ OverrideHappyMode();


            global Event scene7EndScene;
            scene7EndScene => now; 
            
            playNotesShred.exit();
            playShakersShred.exit();

            // reverb tails
            10::second => now;
            

        ", jumpDelay, myJumpEvent, notesString, myIncreaseSpeedEvent, myDecreaseSpeedEvent, myNextLoudness ) );
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

    public void InformOfNextDistance( float distance )
    {
        myChuck.SetFloat( myNextLoudness, distance.PowMapClamp( 0, 1, 1, 0.3f, 2 ) );
    }
}
