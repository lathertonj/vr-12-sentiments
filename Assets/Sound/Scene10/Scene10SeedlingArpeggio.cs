using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene10SeedlingArpeggio : MonoBehaviour
{
	ChuckSubInstance myChuck;
	public float[] cutoffs;

    // Use this for initialization
    void Start()
    {
		myChuck = GetComponent<ChuckSubInstance>();

		myChuck.RunCode( string.Format( @"
            ModalBar modey => JCRev r => HPF hpf => LPF lpf => dac;
            // disable hpf, lpf
            20 => hpf.freq;
			20000 => lpf.freq;

            // set the gain
            .9 => r.gain;
            // set the reverb mix
            .05 => r.mix;

			0.5 => global float scene10NoteLengthSeconds;
			0.06::second => dur postNoteEventBroadcastTime;
			global Event scene10NoteHappened;
			global Event scene10ActualNoteHappened;
            true => int hardPick;

			64 => int E4;
			66 => int Fs4;
            69 => int A4;
            71 => int B4;
            73 => int Cs5;
            76 => int E5;
            78 => int Fs5;
            80 => int Gs5;
            81 => int A5;
            83 => int B5;
            85 => int Cs6;
			87 => int Ds6;
			88 => int E6;

            [[E4, Fs4], [E4, Fs4, A4], [0, Fs4, A4]] @=> int bases[][];
            [[B4, Cs5], [Gs5], [Fs5], [Fs5, Gs5], [Cs5, E5], [E5, Fs5, Gs5], [B4, E5], [B4, E5, Fs5]] @=> int tops[][];
            [[B5], [B5, Cs6], [Cs6, Ds6], [Ds6], [Ds6, E6]] @=> int supertops[][];


			fun void SendOutTatum()
			{{
				while( true )
				{{
					//  this is used for rhythm and so it happens in any case
					scene10ActualNoteHappened.broadcast();
					scene10NoteLengthSeconds::second => now;
				}}
			}}
			spork ~ SendOutTatum();

            fun void PlayArray( int notes[] )
            {{
                2 => modey.preset; // I like 6 and 2
                for( int i; i < notes.size(); i++ )
                {{
                    if( notes[i] > 10 )
                    {{
						// strike position
                        Math.random2f( 0.2, 0.8 ) => modey.strikePosition;
                        // freq
                        notes[i] => Std.mtof => modey.freq;
                        // strike it!
                        Math.random2f( 0.3, 0.4 ) + 0.17 * hardPick => modey.strike;
                        // next pick in opposite direction
                        !hardPick => hardPick;

						// tell some listeners a little late
						postNoteEventBroadcastTime => now;
						scene10NoteHappened.broadcast();
                    }}
                    else
                    {{
						postNoteEventBroadcastTime => now;

                        // next pick in stronger direction
                        true => hardPick;
                    }}
        
                    scene10ActualNoteHappened => now;
        
                    // turn off? this isn't it
                    1 => modey.damp;
                }}
    

            }}


            // our main loop
            global float vibrationAccumulation;

            fun void PlayMainLoop()
            {{
                while( true )
                {{
                    // play a base
                    PlayArray( bases[ Math.random2( 0, bases.size() - 1 ) ] );
    
                    // with small chance, don't play a top (== play a base twice in a row)
                    if( Math.randomf() < 0.2 )
                    {{
                        continue;
                    }}

                    // below the first cutoff, only play bottoms
                    if( vibrationAccumulation < {0} )
                    {{
                        continue;
                    }}
    
                    // with a medium chance, rest once
                    if( Math.randomf() < 0.45 )
                    {{
                        scene10ActualNoteHappened => now;
                    }}
    
                    // play a top
                    PlayArray( tops[ Math.random2( 0, tops.size() - 1 ) ] );
    
                    // with a small chance, play a super top
                    // PARAMETER TO MODULATE: freq of super top
                    0.0 => float superTopChance;
                    0 => int maxSuperTopAllowed;
                    if( vibrationAccumulation > {3} )
                    {{
                        0.6 => superTopChance;
                        supertops.size() - 1 => maxSuperTopAllowed;
                    }}
                    else if( vibrationAccumulation > {2} )
                    {{
                        0.4 => superTopChance;
                        1 => maxSuperTopAllowed;
                    }}
                    else if( vibrationAccumulation > {1} )
                    {{
                        0.2 => superTopChance;
                        0 => maxSuperTopAllowed;
                    }}
                    if( Math.randomf() < superTopChance ) 
                    {{
                        // PARAMETER TO MODULATE: upper limit of allowed super top 
                        // (the array should only get more and more intense left to right)
                        PlayArray( supertops[ Math.random2( 0, maxSuperTopAllowed ) ] );
                    }}
    
                    // with a small chance, rest a while
                    if( Math.randomf() < 0.05 )
                    {{
						repeat( Math.random2( 1, 3 ) )
						{{
							scene10ActualNoteHappened => now;
						}}
                    }}
        
                }}
            }}
            spork ~ PlayMainLoop();
            
            global Event scene10AdvanceToScene11;
            scene10AdvanceToScene11 => now;
            
            // TODO: do something 
            while( true ) 
            {{ 
                //if( hpf.freq() > 20000 ) {{ break; }}
                //hpf.freq() * 1.02 => hpf.freq;
                10::ms => now;
            }}
		", cutoffs[0], cutoffs[1], cutoffs[2], cutoffs[3] ) );

		InvokeRepeating( "DebugSqueezeAmount", 0, 1 );
    }

	private float vibrationAccumulation = 0;
	public ControllerAccessors leftHand, rightHand;

    // Update is called once per frame
    void Update()
    {
		if( leftHand.IsSqueezed() || rightHand.IsSqueezed() )
		{
			vibrationAccumulation += Time.deltaTime;
		}

		myChuck.SetFloat( "vibrationAccumulation", vibrationAccumulation );
		myChuck.SetFloat( "scene10NoteLengthSeconds", vibrationAccumulation.PowMapClamp( 0, cutoffs[cutoffs.Length - 1], 0.5f, 0.12f, pow:0.75f ) );
    }

	void DebugSqueezeAmount()
	{
		Debug.Log( vibrationAccumulation );
	}
}
