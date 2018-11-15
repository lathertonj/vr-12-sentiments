using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene8SqueezeHandSonifier : MonoBehaviour
{

    public int[] myChord;
    public int myIndex;
    private ChuckSubInstance myChuck;
	private string mySqueezedEvent, myUnsqueezedEvent, myAhhGoalVolume;
    private static string myAnySqueezedEvent = "";
    private static int numCurrentlySqueezed = 0;
    public static int numChordChanges = 0;
	private ControllerAccessors myController;
    // Use this for initialization
    void Start()
    {
        myChuck = GetComponent<ChuckSubInstance>();
		myController = GetComponent<ControllerAccessors>();
		mySqueezedEvent = myChuck.GetUniqueVariableName();
		myUnsqueezedEvent = myChuck.GetUniqueVariableName();
        myAhhGoalVolume = myChuck.GetUniqueVariableName();
        if( myAnySqueezedEvent == "" )
        {
            myAnySqueezedEvent = myChuck.GetUniqueVariableName();
        }


		string myChordString = "[";
		for( int i = 0; i < myChord.Length; i++ )
		{
			myChordString += myChord[i].ToString();
			if ( i != myChord.Length - 1 )
			{
				myChordString += ",";
			}
		}
		myChordString += "]";


        // ========================================================================================
        //                                     Supersaw
        // ========================================================================================
		myChuck.RunCode( string.Format( @"
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

            HPF hpf => LPF lpf => Gain ampMod => JCRev reverb => dac;
            400 => hpf.freq;
            800 => lpf.freq; // orig 6000
            0.13 => lpf.gain;
            0.05 => reverb.mix;

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

            61 => int Cs4;
            62 => int D4;
            64 => int E4;
            66 => int Fs4;
            68 => int Gs4;
            69 => int A4;
            71 => int B4;
            73 => int Cs5;
            /*[[Fs4, Fs4, A4, Cs5],
             [Fs4, D4, A4, Cs5],
             [Cs4, E4, Gs4, B4],
             [Fs4, Fs4, Gs4, Cs5]] @=> int myNotes[][];
            [[false, true, true, true],
             [true, true, true, true],
             [true, true, true, true],
             [false, true, true, true]] @=> int shouldPlay[][];
             */
            [[Fs4, A4, Cs5],
             [D4, A4, Cs5],
             [Cs4, Gs4, B4],
             [Fs4, Gs4, Cs5]] @=> int myNotes[][];
            [[true, true, true],
             [true, true, true],
             [true, true, true],
             [true, true, true]] @=> int shouldPlay[][];
            Supersaw mySaws[myNotes[0].size()];
            myNotes.size() - 1 => int currentChord;



            for( int i; i < mySaws.size(); i++ )
            {{
                // how much freq waves up and down as a % of current freq
                1.2 / 300 => mySaws[i].pitchLFODepthMultiplier; 
                // how quickly the freq lfo moves
                3.67 => mySaws[i].pitchLFORate;
                // higher == wider pitch spread
                0.33 => mySaws[i].timbreLFO; 
                // basic loudness
                0.035 => mySaws[i].gain;

                // freq
                myNotes[currentChord][i] => Std.mtof => mySaws[i].freq;
    
                mySaws[i] => lpf; // SKIP hpf
            }}

            0 => float goalGain;
            float currentGains[mySaws.size()];
            0.0006 => float gainSlew;
            false => global int sceneIsOver;
            fun void ApplyGain()
            {{
                while( true )
                {{
                    for( int i; i < mySaws.size(); i++ )
                    {{
                        goalGain => float theGoal;
                        if( sceneIsOver || !shouldPlay[currentChord][i] )
                        {{
                            0 => theGoal;
                        }}
                        gainSlew * ( theGoal - currentGains[i] ) +=> currentGains[i];
                        currentGains[i] => mySaws[i].gain;
                    }}
                    
                    1::ms => now;
                }}
            }}
            spork ~ ApplyGain();

            float goalNotes[mySaws.size()];
            float currentNotes[mySaws.size()];
            0.02 => float noteSlew;
            fun void ApplyFreq()
            {{
                for( int i; i < mySaws.size(); i++ )
                {{
                    myNotes[currentChord][i] => goalNotes[i] => currentNotes[i];
                }}
                while( true )
                {{
                    for( int i; i < mySaws.size(); i++ )
                    {{
                        myNotes[currentChord][i] => goalNotes[i];
                        noteSlew * ( goalNotes[i] - currentNotes[i] ) +=> currentNotes[i];
                        currentNotes[i] => Std.mtof => mySaws[i].freq;
                    }}
                    
                    1::ms => now;
                }}
            }}
            spork ~ ApplyFreq();

            global Event {2};
            fun void RespondToChordChanges()
            {{
                while( true )
                {{
                    {2} => now;
                    currentChord++;
                    if( currentChord >= myNotes.size() )
                    {{
                        0 => currentChord;
                    }}
                }}
            }}
            spork ~ RespondToChordChanges();

            global Event {0}, {1};
            fun void RespondToSqueezes()
            {{
                while( true )
                {{
                    {0} => now;
                    1 => goalGain;
                    {1} => now;
                    0 => goalGain;
                }}
            }}
            spork ~ RespondToSqueezes() @=> Shred squeezeResponseShred;
            
            // turn off chord at end of movement
            global Event scene8EndScene;
            scene8EndScene => now;

            squeezeResponseShred.exit();
            true => sceneIsOver;
            
            // let it die out
            10::second => now;
        ", mySqueezedEvent, myUnsqueezedEvent, myAnySqueezedEvent ) );


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

			AhhSynth myAhh => LPF lpf => NRev rev => dac;
			7000 => lpf.freq;
			rev.mix(0.1);

            {0} => int myIndex;
            73-24 => int CsHigh;
            71-24 => int B;
            69-24 => int A;
            68-24 => int Gs;
            66-24 => int Fs;
            64-24 => int E;
            61-24 => int Cs;
            [[CsHigh, A, B, Gs], [A, Fs, Gs, Cs]] @=> int myNotes[][];
            myNotes[myIndex].size() - 1 => int currentChord;

            fun void SetFreq()
            {{
                float goalNote;
                float currentNote;
                0.1 => float noteSlew;
                myNotes[myIndex][currentChord] => goalNote => currentNote;
                while( true )
                {{
                    myNotes[myIndex][currentChord] => goalNote;
                    noteSlew * ( goalNote - currentNote ) +=> currentNote;
                    currentNote => Std.mtof => myAhh.freq;
                    1::ms => now;
                }}
            }}
            spork ~ SetFreq();

            global Event {2};
            fun void ListenForChordChange()
            {{
                while( true )
                {{
                    {2} => now;
                    currentChord++;
                    if( currentChord >= myNotes[myIndex].size() )
                    {{
                        0 => currentChord;
                    }}
                }}
            }}
            spork ~ ListenForChordChange();
            
            global float {1};
            false => int sceneIsOver;
            fun void SetVolume()
            {{
                0.01 => float volumeUpSlew;
                0.002 => float volumeDownSlew;
                float currentVolume;
                while( true )
                {{
                    {1} => float goalVolume;
                    if( sceneIsOver )
                    {{
                        0 => goalVolume;
                    }}
                    if( goalVolume > currentVolume )
                    {{
                        volumeUpSlew * ( goalVolume - currentVolume ) +=> currentVolume;
                    }}
                    else
                    {{
                        volumeDownSlew * ( goalVolume - currentVolume ) +=> currentVolume;
                    }}
                    currentVolume * 0.16 => myAhh.gain;
                    1::ms => now;
                }}

            }}
            spork ~ SetVolume();

            // let it die out
            global Event scene8EndScene;
            scene8EndScene => now;
            true => sceneIsOver;
			
			// reverb tail
			10::second => now;
        
        ", myIndex, myAhhGoalVolume, myAnySqueezedEvent ));
    }

    // Update is called once per frame
    void Update()
    {
		if( myController.IsFirstSqueezed() )
		{
            if( numCurrentlySqueezed == 0 )
            {
                // tell things to update their note
                myChuck.BroadcastEvent( myAnySqueezedEvent );
                numChordChanges++;
            }
			myChuck.BroadcastEvent( mySqueezedEvent );
            numCurrentlySqueezed++;
		}
		
		if( myController.IsUnSqueezed() )
		{
			myChuck.BroadcastEvent( myUnsqueezedEvent );
            numCurrentlySqueezed--;

		}

        float handSpeed = myController.Velocity().magnitude;
        myChuck.SetFloat( myAhhGoalVolume, handSpeed.MapClamp( 0, 3, 0, 1 ) );
    }
}
