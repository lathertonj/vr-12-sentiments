using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( ChuckSubInstance ) )]
public class SonifyFlowerSeedlings : MonoBehaviour
{
    private ChuckSubInstance myChuck;
    public double[] myChord;

    // Use this for initialization
    void Start()
    {
        StartChuck();
    }

    public void PlayChord()
    {
        if( myChuck != null )
        {
            myChuck.SetFloatArray( myChordNotesVar, myChord );
            myChuck.BroadcastEvent( myADSRStart );
        }
    }

    public void StopChord()
    {
        if( myChuck != null )
        {
            myChuck.BroadcastEvent( myADSREnd );
        }
    }

    private string myChordNotesVar, myADSRStart, myADSREnd;
    private void StartChuck()
    {
        myChuck = GetComponent<ChuckSubInstance>();
        myChordNotesVar = myChuck.GetUniqueVariableName();
        myADSRStart = myChuck.GetUniqueVariableName();
        myADSREnd = myChuck.GetUniqueVariableName();
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
ADSR adsr => ampMod; // should be: hpf
            8000 => lpf.freq; // orig 2000
            50 => hpf.freq; // possibly 1800
            0.05 => reverb.mix;
            0.85 => reverb.gain;
            adsr.set( 0.05::second, 0.05::second, 0.7, 0.1::second );

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

            // TODO: this could use some amplitude modulation
            Supersaw mySaws[4];


            for( int i; i < 4; i++ )
            {{
                // how much freq waves up and down as a % of current freq
                1.2 / 300 => mySaws[i].pitchLFODepthMultiplier; 
                // how quickly the freq lfo moves
                3.67 => mySaws[i].pitchLFORate;
                // higher == wider pitch spread
                0.33 => mySaws[i].timbreLFO; 
                // basic loudness
                0.07 => mySaws[i].gain; // should be: 0.035
    
                mySaws[i] => adsr;
            }}

            [66.0, 69, 73, 76] @=> global float {0}[];
            [66.0, 69, 73, 76] @=> float currentChordNotes[];
            [0.005, 0.0046, 0.0044, 0.0041] @=> float chordSlews[];
            fun void SlewChordFreqs()
            {{
                while( true )
                {{
                    for( int i; i < 4; i++ )
                    {{
                        currentChordNotes[i] + 3 * chordSlews[i] * 
                            ( {0}[i] - currentChordNotes[i] ) => currentChordNotes[i];
                        currentChordNotes[i] - 12 => Std.mtof => mySaws[i].freq;
                    }}
                    1::ms => now;
                }}
            }}
            spork ~ SlewChordFreqs();

            fun void SetLPFCutoff()
            {{
                global float scene3TransitionProgress;
                while( true )
                {{
                    1200 + 4000 * scene3TransitionProgress => lpf.freq;
                    10::ms => now;
                }}
            }}
            spork ~ SetLPFCutoff();


            global Event {1}, {2};
            while( true ) {{
                {1} => now;
                1 => adsr.keyOn;
                {2} => now;
                1 => adsr.keyOff;
            }}
        ", myChordNotesVar, myADSRStart, myADSREnd ) );
        
    }
}
