using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene10SeedlingArpeggio : MonoBehaviour
{
	ChuckSubInstance myChuck;
	public float[] cutoffs;

	public string[] myChord1, myChord2, myChord3; 

	public string[] myAhhChord1, myAhhChord2, myAhhChord3;

	public string[] mySecondHalfChord2, mySecondHalfChord3;
	public string[] myAhhSecondHalfChord2, myAhhSecondHalfChord3;

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
			// 0.045::second => dur postNoteEventBroadcastTime;
			0.04
			::second => dur postNoteEventBroadcastTime;
			true => int shouldListenToUnityTatum;
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
			86 => int D6;
			87 => int Ds6;
			88 => int E6;

            [[E4, Fs4], [E4, Fs4, A4], [0, Fs4, A4]] @=> int bases[][];
            [[B4, Cs5], [Gs5], [Fs5], [Fs5, Gs5], [Cs5, E5], [E5, Fs5, Gs5], [B4, E5], [B4, E5, Fs5]] @=> int tops[][];
            [[B5], [B5, Cs6], [Cs6, Ds6], [Ds6], [Ds6, E6]] @=> int supertops[][];
			

            global Event scene10AdvanceToScene11;
			fun void SendOutTatum()
			{{
				while( shouldListenToUnityTatum )
				{{
					//  this is used for rhythm and so it happens in any case
					scene10ActualNoteHappened.broadcast();
					scene10NoteLengthSeconds::second => now;
				}}
			}}
			spork ~ SendOutTatum();

			fun void SendOutMyOwnTatum()
			{{
				scene10AdvanceToScene11 => now;
				false => shouldListenToUnityTatum;
				1.25 * scene10NoteLengthSeconds => float myOwnNoteLength;
				while( true )
				{{
					myOwnNoteLength::second => now;
					<<< myOwnNoteLength >>>;
					scene10ActualNoteHappened.broadcast();
					1.03 *=> myOwnNoteLength;
					if( myOwnNoteLength > 0.4 )
					{{
						0.4 => myOwnNoteLength;
					}}
				}}
			}}
			spork ~ SendOutMyOwnTatum();

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
                        Math.random2f( 0.5, 0.7 ) + 0.17 * hardPick => modey.strike;
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
            
            scene10AdvanceToScene11 => now;

			// new notes for
			[/*[B5],*/ [B5, Cs6], [Cs6, D6], [D6], [D6, E6]] @=> supertops;
            
            // TODO: do something 
            while( true ) 
            {{ 
                //if( hpf.freq() > 20000 ) {{ break; }}
                //hpf.freq() * 1.02 => hpf.freq;
                10::ms => now;
            }}
		", cutoffs[0], cutoffs[1], cutoffs[2], cutoffs[3] ) );

		myChuck.RunCode( string.Format( @"
			global Event scene10BringInTheClimaxChords, scene10ChordChange, scene10StartListeningForClimax;

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
                0.09 + 0.01 * i => mySaws[i].gain; // orig 0.07
    
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


			scene10StartListeningForClimax => now;
			scene10BringInTheClimaxChords => now;
			repeat( 2 + 3 ) {{ scene10ChordChange => now; }}
			float currentGain;
			1 => float goalGain;
			0.00003 => float gainSlew;

			400 => float currentLPFCutoff;
			10000 => float goalLPFCutoff;
			0.00003 => float cutoffSlew;

			fun void SlewClimaxChords()
			{{
				while( true )
				{{
					gainSlew * ( goalGain - currentGain ) +=> currentGain;
					currentGain => turnOn.gain;

					cutoffSlew * ( goalLPFCutoff - currentLPFCutoff ) +=> currentLPFCutoff;
					currentLPFCutoff => lpf.freq;
					1::ms => now;
				}}
			}}
			spork ~ SlewClimaxChords();

			global Event scene10AdvanceToScene11;

			// wait for a bit (TODO: 13? something else?)
			repeat( 10 ) {{ scene10ChordChange => now; }}

			// Tell everyone to DO THE THING
			scene10AdvanceToScene11.broadcast();

			[[66,66,66,66], [{0}], [{3}], [{4}]] @=> myNotes;

			while( true )
			{{
				1::second => now;
			}}

			
		", string.Join( ",", myChord1 ), string.Join( ",", myChord2 ), string.Join( ",", myChord3 ),
		   string.Join( ",", mySecondHalfChord3 ), string.Join( ",", mySecondHalfChord2 ) ) );


		// ========================================================================================
        //                                     AhhSynth
        // ========================================================================================
        myChuck.RunCode( string.Format( @"
            class AhhSynth extends Chubgraph
			{{
				LiSa lisa => outlet;
				
				// spawn rate: how often a new grain is spawned (ms)
				25 =>  float grainSpawnRateMS;
				0 =>  float grainSpawnRateVariationMS;
				0.0 =>  float grainSpawnRateVariationRateMS;
				
				// position: where in the file is a grain (0 to 1)
				0.61 =>  float grainPosition;
				0.2 =>  float grainPositionRandomness;
				
				// grain length: how long is a grain (ms)
				300 =>  float grainLengthMS;
				10 =>  float grainLengthRandomnessMS;
				
				// grain rate: how quickly is the grain scanning through the file
				1.004 =>  float grainRate; // 1.002 == in-tune Ab
				0.015 =>  float grainRateRandomness;
				
				// ramp up/down: how quickly we ramp up / down
				50 =>  float rampUpMS;
				200 =>  float rampDownMS;
				
				// gain: how loud is everything overall
				1 =>  float gainMultiplier;
				
				float myFreq;
				fun float freq( float f )
				{{
					f => myFreq;
					61 => Std.mtof => float baseFreq;
					// 1.002 == in tune for 56 for aah4.wav
					// 1.004 == in tune for 60 for aah5.wav
					myFreq / baseFreq * 0.98 => grainRate;
					
					return myFreq;
				}}
				
				fun float freq()
				{{
					return myFreq;
				}}
				
				fun float gain( float g )
				{{
					g => lisa.gain;
					return g;
				}}
				
				fun float gain()
				{{
					return lisa.gain();
				}}
				
				
				
				SndBuf buf; 
				me.dir() + ""aah5.wav"" => buf.read;
				buf.length() => lisa.duration;
				// copy samples in
				for( int i; i < buf.samples(); i++ )
				{{
					lisa.valueAt( buf.valueAt( i ), i::samp );
				}}
				
				
				buf.length() => dur bufferlen;
				
				// LiSa params
				100 => lisa.maxVoices;
				0.1 => lisa.gain;
				true => lisa.loop;
				false => lisa.record;
				
				
				// modulate
				SinOsc freqmod => blackhole;
				0.1 => freqmod.freq;
				
				
				
				0.1 => float maxGain;
				
				fun void SetGain()
				{{
					while( true )
					{{
						maxGain * gainMultiplier => lisa.gain;
						1::ms => now;
					}}
				}}
				spork ~ SetGain();
				
				
				fun void SpawnGrains()
				{{
					// create grains
					while( true )
					{{
						// grain length
						( grainLengthMS + Math.random2f( -grainLengthRandomnessMS / 2, grainLengthRandomnessMS / 2 ) )
						* 1::ms => dur grainLength;
						
						// grain rate
						grainRate + Math.random2f( -grainRateRandomness / 2, grainRateRandomness / 2 ) => float grainRate;
						
						// grain position
						( grainPosition + Math.random2f( -grainPositionRandomness / 2, grainPositionRandomness / 2 ) )
						* bufferlen => dur playPos;
						
						// grain: grainlen, rampup, rampdown, rate, playPos
						spork ~ PlayGrain( grainLength, rampUpMS::ms, rampDownMS::ms, grainRate, playPos);
						
						// advance time (time per grain)
						// PARAM: GRAIN SPAWN RATE
						grainSpawnRateMS::ms  + freqmod.last() * grainSpawnRateVariationMS::ms => now;
						grainSpawnRateVariationRateMS => freqmod.freq;
					}}
				}}
				spork ~ SpawnGrains();
				
				// sporkee
				fun void PlayGrain( dur grainlen, dur rampup, dur rampdown, float rate, dur playPos )
				{{
					lisa.getVoice() => int newvoice;
					
					if(newvoice > -1)
					{{
						lisa.rate( newvoice, rate );
						lisa.playPos( newvoice, playPos );
						lisa.rampUp( newvoice, rampup );
						( grainlen - ( rampup + rampdown ) ) => now;
						lisa.rampDown( newvoice, rampdown) ;
						rampdown => now;
					}}
				}}


			}}

			[[66,66,66,66], [{0}], [{1}], [{2}]] @=> int myNotes[][];
			global int myCurrentChord;
            AhhSynth myAhhs[myNotes[0].size()];

			LPF lpf => NRev rev => Gain turnOn => dac;
			rev.mix(0.1);

            for( int i; i < myAhhs.size(); i++ )
            {{
                // basic loudness
                0.035 => myAhhs[i].gain; // should be: 0.035
				0.07 - 0.01 * i => myAhhs[i].gain;
    
				// pitch
				myNotes[myCurrentChord][i] => Std.mtof => myAhhs[i].freq;

                myAhhs[i] => lpf;
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
                    for( int i; i < myAhhs.size(); i++ )
                    {{
                        currentChordNotes[i] + chordSlew * 
                            ( myNotes[myCurrentChord][i] - currentChordNotes[i] ) => currentChordNotes[i];
                        currentChordNotes[i] - 0 => Std.mtof => myAhhs[i].freq;
                    }}
                    1::ms => now;
                }}
            }}
            spork ~ SlewChordFreqs();

			global Event scene10BringInTheClimaxChords, scene10ChordChange, scene10StartListeningForClimax;
			scene10StartListeningForClimax => now;
			scene10BringInTheClimaxChords => now;
			float currentGain;
			1 => float goalGain;
			0.00003 => float gainSlew;

			300 => float currentLPFCutoff;
			7000 => float goalLPFCutoff;
			0.00003 => float cutoffSlew;

			fun void SlewClimaxChords()
			{{
				while( true )
				{{
					gainSlew * ( goalGain - currentGain ) +=> currentGain;
					currentGain => turnOn.gain;

					cutoffSlew * ( goalLPFCutoff - currentLPFCutoff ) +=> currentLPFCutoff;
					currentLPFCutoff => lpf.freq;
					1::ms => now;
				}}
			}}
			spork ~ SlewClimaxChords();

			global Event scene10AdvanceToScene11;
			scene10AdvanceToScene11 => now;
			

			[[66,66,66,66], [{0}], [{3}], [{4}]] @=> myNotes;
			// reinforce bass notes
			1.2 * myAhhs[0].gain() => myAhhs[0].gain;
			1.2 * myAhhs[1].gain() => myAhhs[1].gain;

			while( true )
			{{
				1::second => now;
			}}
        
        ", string.Join( ",", myAhhChord1 ), string.Join( ",", myAhhChord2 ), string.Join( ",", myAhhChord3 ),
		   string.Join( ",", myAhhSecondHalfChord3 ), string.Join( ",", myAhhSecondHalfChord2 ) ) );

		//InvokeRepeating( "DebugSqueezeAmount", 0, 1 );
    }

	private float vibrationAccumulation = 0;
	public ControllerAccessors leftHand, rightHand;


	private bool haveLaunchedClimaxChords = false;
    // Update is called once per frame
    void Update()
    {
		if( leftHand.IsSqueezed() )
		{
			vibrationAccumulation += Time.deltaTime;
		}

		if( rightHand.IsSqueezed() )
		{
			vibrationAccumulation += Time.deltaTime;
		}

		myChuck.SetFloat( "vibrationAccumulation", vibrationAccumulation );
		myChuck.SetFloat( "scene10NoteLengthSeconds", vibrationAccumulation.PowMapClamp( 0, cutoffs[cutoffs.Length - 1], 0.5f, 0.12f, pow:0.75f ) );
	    Scene10WiggleSeedling.SetWiggleMultiplier( vibrationAccumulation.PowMapClamp( 0, cutoffs[cutoffs.Length - 1], 1.0f, 2.0f, pow:2f ) );

		if( !haveLaunchedClimaxChords && vibrationAccumulation > cutoffs[cutoffs.Length - 1] - 5 )
		{
			LaunchClimaxChords();
		}
    }

	void DebugSqueezeAmount()
	{
		Debug.Log( vibrationAccumulation );
	}


	void LaunchClimaxChords()
	{
		haveLaunchedClimaxChords = true;
		myChuck.BroadcastEvent( "scene10StartListeningForClimax" );
	}
}
