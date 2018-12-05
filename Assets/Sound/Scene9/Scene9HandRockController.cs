﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene9HandRockController : MonoBehaviour
{
	private ControllerAccessors myController;
	private ChuckSubInstance myChuck;
	public long[] myModalNotes;
	public string[] myChordNotes;
	private string myNotesVar;
	private string myGoalGainVar;
	private string myPlayNoteVar;

    // Use this for initialization
    void Start()
    {
		myController = GetComponent<FollowObject>().objectToFollow.GetComponent<ControllerAccessors>();
		myChuck = GetComponent<ChuckSubInstance>();
		myNotesVar = myChuck.GetUniqueVariableName();
		myPlayNoteVar = myChuck.GetUniqueVariableName();
		myGoalGainVar = myChuck.GetUniqueVariableName();

		// =================================================================
		// =                          MODALBAR                             =
		// =================================================================
		myChuck.RunCode( string.Format( @"
			global int {0}[1];
            global Event {1};
			global ModalBar modey => JCRev reverb => dac;
            0.15::second => dur minNoteLength;

            true => int playStrong;

            fun void PlayNotes()
            {{
                while( true )
                {{
					for( int i; i < {0}.size(); i++ )
					{{
						// wait until told to play next note
						{1} => now;

						// play note
						if( {0}[i] > 10 )
						{{
							Math.random2f( 0.2, 0.8 ) => modey.strikePosition;
							{0}[i] => Std.mtof => modey.freq;

							me.yield();

							Math.random2f( 0.3, 0.4 ) + playStrong * 0.17 => modey.strike;
							!playStrong => playStrong;
						}}

						// wait for next note
						minNoteLength => now;
					}}
                    
                }}
            }}
			spork ~ PlayNotes();

			while( true ) {{ 1::second => now; }}
		
		", myNotesVar, myPlayNoteVar ) );
		myChuck.SetIntArray( myNotesVar, myModalNotes );


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
            1800 => hpf.freq;
            1000 => lpf.freq; // orig 6000
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

            69 => int A4;
            73 => int Cs5;
            76 => int E5;
            83 => int B5;
            90 => int Fs5;

            [{0}] @=> int myNotes[];
            Supersaw mySaws[myNotes.size()];



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
                myNotes[i] => Std.mtof => mySaws[i].freq;
    
                mySaws[i] => lpf; // SKIP hpf
            }}

			global float {1};
            0 => float goalGain;
            0 => float currentGain;
            0.0006 => float gainSlew;
			true => int respondToGain;
            fun void ApplyGain()
            {{
                while( true )
                {{
					if( respondToGain )
					{{
						{1} => goalGain;
					}}
                    gainSlew * ( goalGain - currentGain ) +=> currentGain;
                    currentGain => lpf.gain;
                    500 + 5000 * currentGain => lpf.freq;
                    1::ms => now;
                }}
            }}
            spork ~ ApplyGain();

            
            // TODO:
            // turn off chord at end of movement
            global Event scene9EndEvent;
            scene9EndEvent => now;
            //squeezeResponseShred.exit();
			false => respondToGain;
            0 => goalGain;
            
            // let it die out
            while( true ) 
            {{ 
                0 => goalGain;
                1::second => now;
            }} 
        ", string.Join( ",", myChordNotes ), myGoalGainVar ) );
    }

	void Update()
	{
		float handSpeed = myController.Velocity().magnitude;
        myChuck.SetFloat( myGoalGainVar, handSpeed.MapClamp( 0, 0.7f, 0, 1 ) );
	}

    public void InformHit()
	{
		myController.Vibrate( 500 );
		myChuck.BroadcastEvent( myPlayNoteVar );
	}
}
