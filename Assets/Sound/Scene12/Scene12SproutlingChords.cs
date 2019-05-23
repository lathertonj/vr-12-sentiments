using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene12SproutlingChords : MonoBehaviour
{
    public string[] notes;
    private ChuckSubInstance myChuck;
    private NPCLeafController3 myLeaf;
    private string myLoudness;
    private float fadeInVolume = 0f;

    // Use this for initialization
    void Start()
    {
        myLeaf = GetComponentInChildren<NPCLeafController3>();
        myChuck = GetComponent<ChuckSubInstance>();
        myLoudness = myChuck.GetUniqueVariableName();
        StartCoroutine( FadeInVolume( 3f ) );
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

            HPF hpf => LPF lpf => Gain ampMod => JCRev reverb => Gain volumeControl => dac;
            1800 => hpf.freq;
            2000 => lpf.freq; // orig 6000
            200 => hpf.freq;
            0.05 => reverb.mix;
            0 => volumeControl.gain; // off by default
            global float {0};

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
            [{1}] @=> int chordNotes[];
            Supersaw mySaws[chordNotes.size()];

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
                chordNotes[i] => Std.mtof => mySaws[i].freq;
                //<<< chordNotes[i]>>>;
    
                mySaws[i] => hpf;
            }}

            fun void RespondToLoudness()
            {{
                while( true )
                {{
                    {0} => volumeControl.gain;
                    10::ms => now;
                }}
            }}
            spork ~ RespondToLoudness();

            global Event scene12Finish;

            scene12Finish => now;
            
        ", myLoudness, string.Join( ",", notes ) ) );
    }

    IEnumerator FadeInVolume( float fadeInTime )
    {
        yield return new WaitForSecondsRealtime( 0.5f );
        float startTime = Time.time;
        while( Time.time < startTime + fadeInTime )
        {
            fadeInVolume = ( Time.time - startTime ).PowMapClamp( 0, fadeInTime, 0, 1, 2f );
            yield return null;
        }
        fadeInVolume = 1;
    }

    // Update is called once per frame
    void Update()
    {
        // set loudness
        myChuck.SetFloat( myLoudness, fadeInVolume * myLeaf.myHeight.Map( -1, 1, 0, 1 ) );
    }
}
