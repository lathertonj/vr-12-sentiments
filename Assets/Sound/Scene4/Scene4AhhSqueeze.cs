using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene4AhhSqueeze : MonoBehaviour
{
	ChuckSubInstance myChuck;
	ControllerAccessors myController;
	public int myAhhNote;
	private string myAhhGoalVolume;
    // Use this for initialization
    void Start()
    {
		myController = GetComponent<ControllerAccessors>();
		myChuck = GetComponent<ChuckSubInstance>();
		myAhhGoalVolume = myChuck.GetUniqueVariableName();

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

            {0} => Std.mtof => myAhh.freq;
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
                    currentVolume * 0.06 => myAhh.gain;
                    1::ms => now;
                }}

            }}
            spork ~ SetVolume();

            // let it die out
            global Event scene4EndEvent;
            scene4EndEvent => now;
            true => sceneIsOver;
			
			// reverb tail
			10::second => now;
        
        ", myAhhNote, myAhhGoalVolume ));
    }

    // Update is called once per frame
    void Update()
    {
		float handSpeed = myController.Velocity().magnitude;
        myChuck.SetFloat( myAhhGoalVolume, handSpeed.MapClamp( 0, 3, 0, 1 ) );
    }
}
