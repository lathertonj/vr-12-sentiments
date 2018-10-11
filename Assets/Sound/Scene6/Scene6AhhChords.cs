﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene6AhhChords : MonoBehaviour
{
	private ChuckSubInstance myChuck;

    // Use this for initialization
    void Start()
    {
		myChuck = GetComponent<ChuckSubInstance>();
		myChuck.RunCode( @"
			//-----------------------------------------------------------------------------
			// name: LiSa-Sndbuf2.ck
			// desc: Live sampling utilities for ChucK
			//
			// author: Jack Atherton
			//
			// Combining Dan Trueman's various helper scripts for Lisa
			// https://github.com/ccrma/music220a/blob/master/chuck-examples/special/LiSa-SndBuf.ck
			// http://chuck.cs.princeton.edu/doc/examples/special/LiSa-munger2.ck
			//-----------------------------------------------------------------------------

			// PARAMS
			class AhhSynth extends Chubgraph
			{
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
				{
					f => myFreq;
					60 => Std.mtof => float baseFreq;
					// 1.002 == in tune for 56 for aah4.wav
					// 1.004 == in tune for 60 for aah5.wav
					myFreq / baseFreq * 1.004 => grainRate;
					
					return myFreq;
				}
				
				fun float freq()
				{
					return myFreq;
				}
				
				fun float gain( float g )
				{
					g => lisa.gain;
					return g;
				}
				
				fun float gain()
				{
					return lisa.gain();
				}
				
				
				
				SndBuf buf; 
				me.dir() + ""aah5.wav"" => buf.read;
				buf.length() => lisa.duration;
				// copy samples in
				for( int i; i < buf.samples(); i++ )
				{
					lisa.valueAt( buf.valueAt( i ), i::samp );
				}
				
				
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
				{
					while( true )
					{
						maxGain * gainMultiplier => lisa.gain;
						1::ms => now;
					}
				}
				spork ~ SetGain();
				
				
				fun void SpawnGrains()
				{
					// create grains
					while( true )
					{
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
					}
				}
				spork ~ SpawnGrains();
				
				// sporkee
				fun void PlayGrain( dur grainlen, dur rampup, dur rampdown, float rate, dur playPos )
				{
					lisa.getVoice() => int newvoice;
					
					if(newvoice > -1)
					{
						lisa.rate( newvoice, rate );
						lisa.playPos( newvoice, playPos );
						lisa.rampUp( newvoice, rampup );
						( grainlen - ( rampup + rampdown ) ) => now;
						lisa.rampDown( newvoice, rampdown) ;
						rampdown => now;
					}
				}


			}

			LPF lpf => NRev rev => dac;
			7000 => lpf.freq;
			rev.mix(0.1);

			57 => int A3;
			59 => int B3;
			61 => int Cs4;
			64 => int E4;
			66 => int Fs4;
			68 => int Gs4;
			71 => int B4;
			73 => int Cs5;
			75 => int Ds5;
			78 => int Fs5;
			80 => int Gs5;
			83 => int B5;
			E4 - 12 => int E3;
			E4 + 12 => int E5;
			Gs4 - 12 => int Gs3;

			// beginning of scene notes
			[[Cs4 - 24, Gs4 - 12, E5 - 12, Cs4 - 12],
			[Gs3 - 12, Cs4 - 12, Fs5 - 12, Cs4 - 12]] @=> int notes[][];

			// the actual notes
			[ 
			[A3, E4, Cs5, Gs5],
			[E3, B4, Ds5, Gs5],
			[Cs4 - 12, Gs4, E5, B5],
			[Gs3, Cs4, B4, Fs5]
			] @=> int notes2[][];

			AhhSynth ahhs[ notes[0].size() ];
			for( int i; i < ahhs.size(); i++ )
			{
				0.8 / ahhs.size() => ahhs[i].gain;
				ahhs[i] => lpf;
			}

			global float scene6SwellIntensity;
			global Event scene6SwellStart;
			fun void PopulateSwellIntensity()
			{
				while( true )
				{
					1::ms => now;
					lpf.gain() => scene6SwellIntensity;
				}
			}
			spork ~ PopulateSwellIntensity();

			fun void DoSwell( dur swellUp, dur sustain, dur swellDown, float max, float end )
			{
				scene6SwellStart.broadcast();
				lpf.gain() => float start;

				now => time startTime;
				while( now - startTime < swellUp )
				{
					( now - startTime ) / swellUp => float elapsed;
					start + ( max - start ) * elapsed => lpf.gain;
					1::ms => now;
				}	

				1 => lpf.gain;
				sustain => now;

				now => startTime;
				while( now - startTime < swellDown )
				{
					( now - startTime ) / swellDown => float elapsed;
					max + ( end - max ) * elapsed => lpf.gain;
					1::ms => now;
				}
				end => lpf.gain;
			}

			false => global int halfwayThroughScene6Change;

			global Event scene6TimesWhenWeMightDoSceneChange;
			global Event ahhChordChange;
			global Event ahhChordFadeOut;
			// wait at start of scene
			0 => lpf.gain;
			8::second => now;
			while( true )
			{
				if( halfwayThroughScene6Change )
				{
					notes2 @=> notes;
					// increment it to 2 so now we know we've heard it! hacky bool --> int
					1 +=> halfwayThroughScene6Change;
				}

				for( int i; i < notes.size(); i++ )
				{
					for( int j; j < notes[i].size(); j++ )
					{
						// TODO: maybe start the scene with some stuff down the octave (-24).... then switch it to this up the octave stuff (-12)!!
						notes[i][j] - 12 => Std.mtof => ahhs[j].freq;
					}

					// 2::second => now;
					if( halfwayThroughScene6Change > 1 ) 
					{
						// only tell rocks to change colors after halfway through scene change
						ahhChordChange.broadcast();
						2::second => dur upTime;
						1::second => dur sustainTime;
						3.5::second => dur downTime;
						1::second => dur waitTime;
						spork ~ DoSwell( upTime, sustainTime, downTime, 1, 0.0 );
						upTime + sustainTime => now;
						downTime => now;
						ahhChordFadeOut.broadcast();
						// wait / or not!
						waitTime => now;
					}
					else
					{
						DoSwell( 3.5::second, 0::second, 4.5::second, 0.5, 0.01 );
						// wait
						3.5::second => now;
						
						// check
						if( i == notes.size() - 1 )
						{
							scene6TimesWhenWeMightDoSceneChange.broadcast();
						}
							
						// finish waiting
						0.5::second => now;
					}
				}
			}
		" );

		ChuckEventListener mySecondHalfAdvancer = gameObject.AddComponent<ChuckEventListener>();
		mySecondHalfAdvancer.ListenForEvent( myChuck, "scene6TimesWhenWeMightDoSceneChange", CheckIfWeShouldDoSceneChange );
    }

	bool haveSwitchedToSecondHalf = false;
	void CheckIfWeShouldDoSceneChange()
	{
		if( !haveSwitchedToSecondHalf && Scene6DetectSunLook.sunLookAmount > 5 && Scene6DetectSunLook.currentlyLookingAtSun )
		{
			// signal that the switch should happen at the next musically relevant place
			myChuck.SetInt( "halfwayThroughScene6Change", 1 );
			haveSwitchedToSecondHalf = true;
		}
	}


}
