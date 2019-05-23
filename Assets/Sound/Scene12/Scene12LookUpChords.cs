using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene12LookUpChords : MonoBehaviour
{
    public string[] ahhChord1, ahhChord2;
    private ChuckSubInstance myChuck;
    private bool haveRunChuck = false;
    // Start is called before the first frame update
    void Start()
    {
        myChuck = GetComponent<ChuckSubInstance>();
        RunChuck();
    }

    // Update is called once per frame
    void Update()
    {
        if( Scene12LookUpParticleSystemManager.haveTransitioned && !haveRunChuck )
        {
            myChuck.BroadcastEvent( "scene12StartPlayingWind" );
            haveRunChuck = true;
        }
    }

    void RunChuck()
    {
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
            [[{0}],[{1}]] @=> int myChords[][];

			AhhSynth myAhhs[myChords[0].size()];
            Gain ahhGain => LPF lpf => NRev rev => dac;
			15000 => lpf.freq;
			rev.mix(0.1);
            ahhGain.gain(0);

            0 => int currentChord;

            fun void LFOs()
            {{
                SinOsc lfos[ myAhhs.size() ];
                SinOsc slowLFOs[ myAhhs.size() ];
                for( int i; i < lfos.size(); i++ )
                {{
                    0.5 + 0.33 * i => lfos[i].freq;
                    Math.random2f( 0.1, 0.3 ) => slowLFOs[i].freq;
                    lfos[i] => blackhole;
                    slowLFOs[i] => blackhole;
                }}

                while( true )
                {{
                    for( int i; i < myAhhs.size(); i++ )
                    {{
                        Std.mtof( myChords[currentChord][i] - 12 ) * Std.scalef( lfos[i].last(), -1, 1, 0.99, 1.01 ) => myAhhs[i].freq;
                        Std.scalef( slowLFOs[i].last(), -1, 1, 0.8, 1.0 ) => myAhhs[i].gain;
                    }}
                    10::ms => now;
                }}
            }}
            spork ~ LFOs();

            float scene12GoalVolume;
            false => int sceneIsOver;
            fun void SetVolume()
            {{
                0.0003 => float volumeUpSlew;
                0.0003 => float volumeDownSlew;
                float currentVolume;
                while( true )
                {{
                    scene12GoalVolume => float goalVolume;
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
                    currentVolume * 0.9 => ahhGain.gain;
                    1::ms => now;
                }}

            }}
            spork ~ SetVolume();


            fun void PlaySwells()
            {{
                while( true )
                {{
                    0 => currentChord;
                    PlaySwell();

                    1 => currentChord;
                    PlaySwell();
                }}
            }}

            global Event scene12StartWind, scene12StopWind;
            fun void PlaySwell()
            {{
                1 => scene12GoalVolume;
                scene12StartWind.broadcast();

                Math.random2f(7, 9)::second => now;
                0 => scene12GoalVolume;
                scene12StopWind.broadcast();

                Math.random2f(11,13)::second => now;
            }}

            global Event scene12StartPlayingWind;
            scene12StartPlayingWind => now;

            // connect winds
            for( int i; i < myAhhs.size(); i++ )
            {{
                myAhhs[i] => ahhGain;
            }}

            // play swells
            spork ~ PlaySwells();


            // let it die out
            global Event scene12Finish;
            scene12Finish => now;
            true => sceneIsOver;
			
			// reverb tail
			10::second => now;
        
        ",
            string.Join( ",", ahhChord1 ),
            string.Join( ",", ahhChord2 )
        ) );
    }
}
