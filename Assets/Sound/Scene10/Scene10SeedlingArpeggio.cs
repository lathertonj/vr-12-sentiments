using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene10SeedlingArpeggio : MonoBehaviour
{
	ChuckSubInstance myChuck;
	public float[] cutoffs;

	public string[] myChord1, myChord2, myChord3; 

    // Use this for initialization
    void Start()
    {
		myChuck = GetComponent<ChuckSubInstance>();
LaunchClimaxChords();
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


	private bool haveLaunchedClimaxChords = false;
    // Update is called once per frame
    void Update()
    {
		if( leftHand.IsSqueezed() || rightHand.IsSqueezed() )
		{
			vibrationAccumulation += Time.deltaTime;
		}

		myChuck.SetFloat( "vibrationAccumulation", vibrationAccumulation );
		myChuck.SetFloat( "scene10NoteLengthSeconds", vibrationAccumulation.PowMapClamp( 0, cutoffs[cutoffs.Length - 1], 0.5f, 0.12f, pow:0.75f ) );

		if( !haveLaunchedClimaxChords && vibrationAccumulation > cutoffs[cutoffs.Length - 1] )
		{
			LaunchClimaxChords();
			haveLaunchedClimaxChords = true;
		}
    }

	void DebugSqueezeAmount()
	{
		Debug.Log( vibrationAccumulation );
	}


	void LaunchClimaxChords()
	{
		myChuck.RunCode( string.Format( @"
			global Event scene10BringInTheClimaxChords, scene10ChordChange;

			// using a carrier wave (saw oscillator for example,) 
            // and modulating its signal using a comb filter 
            // where the filter cutoff frequency is usually 
            // modulated with an LFO, which the LFO's depth (or amplitude) 
            // is equal to the saw oscillator's current frequency. 


            // It can also be done by using a copied signal and 
            // have the copy run throught a delay which the 
            // delay's time is modulated again by an LFO where the 
            // LFO's depth is equal to the saw oscillator's current frequency.

            class Supersaw extends Chubgraph
            {{
                SawOsc osc => Gain out => outlet;
                5 => int numDelays;
                1.0 / (1 + numDelays) => out.gain;
                DelayA theDelays[numDelays];
                SinOsc lfos[numDelays];
                dur baseDelays[numDelays];
                float baseFreqs[numDelays];
                for( int i; i < numDelays; i++ )
                {{
                    osc => theDelays[i] => out;
                    0.15::second => theDelays[i].max;
                    // crucial to modify!
                    Math.random2f( 0.001, 0.002 )::second => baseDelays[i];
                    lfos[i] => blackhole;
            //        Math.random2f( -0.1, 0.1) => baseFreqs[i];
                    Math.random2f( 0, pi ) => lfos[i].phase;
        
                }}
                0.05::second => dur baseDelay;
                0.333 => float baseFreq;
                1 => float lfoGain;
    
                fun void AttachLFOs()
                {{
                    while( true )
                    {{
                        for( int i; i < numDelays; i++ )
                        {{
                            baseFreq + baseFreqs[i] => lfos[i].freq;
                            lfoGain * lfos[i].last()::second + baseDelay + baseDelays[i] 
                                => theDelays[i].delay;
                        }}
                        1::ms => now;
                    }}
                }}
                spork ~ AttachLFOs();
    
    
                SinOsc pitchLFO => blackhole;
                0.77 => pitchLFO.freq;
                1.0 / 300 => float pitchLFODepth;
                440 => float basePitch;
    
                fun void FreqMod()
                {{
                    while( true )
                    {{
                        // calc freq
                        basePitch + ( basePitch * pitchLFODepth ) 
                            * pitchLFO.last() => float f;
                        // set
                        f => osc.freq;
                        1.0 / f => lfoGain; // seconds per cycle == gain amount
                        // wait
                        1::ms => now;
                    }}
                }}
                spork ~ FreqMod();
    
                fun void freq( float f )
                {{
                    f => basePitch;
                }}
    
                fun void delay( dur d )
                {{
                    d => baseDelay;
                }}
    
                fun void timbreLFO( float f )
                {{
                    f => baseFreq;
                }}
    
                fun void pitchLFORate( float f )
                {{
                    f => pitchLFO.freq;
                }}
    
                fun void pitchLFODepthMultiplier( float r )
                {{
                    r => pitchLFODepth;
                }}
            }}

            HPF hpf => LPF lpf => Gain ampMod => JCRev reverb => Gain turnOn => dac;
            15000 => lpf.freq; // orig 2000
            50 => hpf.freq; // possibly 1800
            0.05 => reverb.mix;
            0.85 => reverb.gain;

			0 => turnOn.gain;

            fun void AmpMod()
            {{
                SinOsc lfo => blackhole;
                0.13 => lfo.freq;
                while( true )
                {{
                     0.85 + 0.15 * lfo.last() => ampMod.gain;
                     1::ms => now;
                }}
            }}
            spork ~ AmpMod();

			[[66,66,66,66], [{0}], [{1}], [{2}]] @=> int myNotes[][];
			global int myCurrentChord;
            Supersaw mySaws[myNotes[0].size()];


            for( int i; i < mySaws.size(); i++ )
            {{
                // how much freq waves up and down as a % of current freq
                1.2 / 300 => mySaws[i].pitchLFODepthMultiplier; 
                // how quickly the freq lfo moves
                3.67 => mySaws[i].pitchLFORate;
                // higher == wider pitch spread
                0.33 => mySaws[i].timbreLFO; 
                // basic loudness
                0.07 => mySaws[i].gain; // should be: 0.035
    
				// pitch
				myNotes[myCurrentChord][i] => Std.mtof => mySaws[i].freq;

                mySaws[i] => hpf;
            }}

            float currentChordNotes[myNotes[0].size()];
            for( int i; i < currentChordNotes.size(); i++ )
            {{
                myNotes[myCurrentChord][i] => currentChordNotes[i];
            }}
            0.05 => float chordSlew;
            fun void SlewChordFreqs()
            {{
                while( true )
                {{
                    for( int i; i < mySaws.size(); i++ )
                    {{
                        currentChordNotes[i] + chordSlew * 
                            ( myNotes[myCurrentChord][i] - currentChordNotes[i] ) => currentChordNotes[i];
                        currentChordNotes[i] - 0 => Std.mtof => mySaws[i].freq;
                    }}
                    1::ms => now;
                }}
            }}
            spork ~ SlewChordFreqs();

			scene10BringInTheClimaxChords => now;
			float currentGain;
			1 => float goalGain;
			0.00003 => float gainSlew;

			400 => float currentLPFCutoff;
			10000 => float goalLPFCutoff;
			0.00003 => float cutoffSlew;

			while( true )
			{{
				gainSlew * ( goalGain - currentGain ) +=> currentGain;
				currentGain => turnOn.gain;

				cutoffSlew * ( goalLPFCutoff - currentLPFCutoff ) +=> currentLPFCutoff;
				currentLPFCutoff => lpf.freq;
				1::ms => now;
			}}
		", string.Join( ",", myChord1 ), string.Join( ",", myChord2 ), string.Join( ",", myChord3 ) ) );
	}
}
