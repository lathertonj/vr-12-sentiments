using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( ChuckSubInstance ) )]
public class SonifyFlowerSeedlings : MonoBehaviour
{
    private ChuckSubInstance myChuck;
    public double[] myChord;

    private string myChordNotesVar;
    public void StartChuck( string startChordEvent, string stopChordEvent, string tatum, string squeezedEvent, string unsqueezedEvent )
    {
        myChuck = GetComponent<ChuckSubInstance>();
        myChordNotesVar = myChuck.GetUniqueVariableName();
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
ADSR adsr => lpf; // should be: hpf
            8000 => lpf.freq; // orig 2000
            50 => hpf.freq; // possibly 1800
            0.05 => reverb.mix;
            0.85 => reverb.gain;
            adsr.set( 0.01::second, 0.01::second, 0.7, 0.1::second );

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

            Supersaw mySaws[{3}];


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
    
                mySaws[i] => adsr;
            }}

            global float {0}[{3}];
            float currentChordNotes[{3}];
            for( int i; i < {0}.size(); i++ )
            {{
                66 => {0}[i];
                66 => currentChordNotes[i];
            }}
            0.5 => float chordSlew;
            fun void SlewChordFreqs()
            {{
                while( true )
                {{
                    for( int i; i < mySaws.size(); i++ )
                    {{
                        currentChordNotes[i] + chordSlew * 
                            ( {0}[i] - currentChordNotes[i] ) => currentChordNotes[i];
                        currentChordNotes[i] - 0 => Std.mtof => mySaws[i].freq;
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
                    15000 + 8000 * scene3TransitionProgress => lpf.freq;
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
        ", myChordNotesVar, startChordEvent, stopChordEvent, myChord.Length ) );
        myChuck.SetFloatArray( myChordNotesVar, myChord );


        myChuck.RunCode( string.Format( @"
            global float {0};
            global Event {1}, {2};
            // 0: tatum in seconds, filled in by another shred
            // 1: squeezed event
            // 2: unsqueezed event

            Shakers s => JCRev reverb => dac;
            0.05 => reverb.mix;

            // params
            0.7 => s.decay; // 0 to 1
            50 => s.objects; // 0 to 128
            11 => s.preset; // 0 to 22.  0 is good for galloping. 
            // 11 is good for eighths

            fun void Play()
            {{
                while( true )
                {{
                    me.yield();
                    {0}::second => dur tatum; // TODO duplicate below if have syncing issues, but I don't expect any
                    
                    me.yield();
                    Math.random2f( 0.7, 0.9 ) => s.energy;
                    1 => s.noteOn;
                    1::tatum => now;

                    me.yield();
                    Math.random2f( 0.1, 0.3 ) => s.energy;
                    1 => s.noteOn;
                    1::tatum => now;
    
                    me.yield();
                    Math.random2f( 0.3, 0.5 ) => s.energy;
                    1 => s.noteOn;
                    1::tatum => now;

                    me.yield();
                    Math.random2f( 0.1, 0.3 ) => s.energy;
                    1 => s.noteOn;
                    1::tatum => now;
                }}

            }}

            while( true )
            {{
                {1} => now;
                spork ~ Play() @=> Shred playShred;
                {2} => now;
                playShred.exit();
            }}
            
            
        ", tatum, squeezedEvent, unsqueezedEvent ) );
    }
}
