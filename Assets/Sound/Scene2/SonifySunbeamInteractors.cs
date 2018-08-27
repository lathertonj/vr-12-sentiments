using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonifySunbeamInteractors : MonoBehaviour
{
    public SunbeamInteractors otherInteractor;
    private SunbeamInteractors myInteractor;
    private SonifySunbeamInteractors otherSonifier;
    private ChuckSubInstance myChuck;
    private bool iAmPlaying = false;

    public float bassNote1, bassNote2;
    public double[] chord1, chord2;
    private bool secondHalf = false;

    // Use this for initialization
    void Start()
    {
        myChuck = GetComponent<ChuckSubInstance>();
        myInteractor = GetComponent<SunbeamInteractors>();
        otherSonifier = otherInteractor.GetComponent<SonifySunbeamInteractors>();

        StartChuck();
    }

    // Update is called once per frame
    void Update()
    {
        ushort myStrength = myInteractor.CurrentStrength();
        if( !iAmPlaying && myStrength > 0 && myStrength > otherInteractor.CurrentStrength() )
        {
            // turn on the thing
            TurnOn();

            // tell the other one to turn off
            otherSonifier.TurnOff();
        }

        double myCurrentGain = myInteractor.CurrentStrength() / 3999f;
        if( iAmPlaying )
        {
            myChuck.SetFloat( "bassGain", myCurrentGain );
        }

        myChuck.SetFloat( myGainVar, myCurrentGain );
    }

    // called by my event listener every 16 tatums
    private void UpdateChordTones()
    {
        if( !secondHalf ) return;

        if( !haveIncrementedChordTones[3] && 
            SunbeamInteractors.sunbeamAccumulated - sunbeamAmountAtSecondHalf > 20 )
        {
            chord2[3] += 12;
            myChuck.SetFloatArray( myChordNotesVar, chord2 );
            haveIncrementedChordTones[3] = true;
        }
        else if( !haveIncrementedChordTones[2] && 
            SunbeamInteractors.sunbeamAccumulated - sunbeamAmountAtSecondHalf > 15 )
        {
            chord2[2] += 12;
            myChuck.SetFloatArray( myChordNotesVar, chord2 );
            haveIncrementedChordTones[2] = true;
        }
        else if( !haveIncrementedChordTones[1] && 
            SunbeamInteractors.sunbeamAccumulated - sunbeamAmountAtSecondHalf > 10 )
        {
            chord2[1] += 12;
            myChuck.SetFloatArray( myChordNotesVar, chord2 );
            haveIncrementedChordTones[1] = true;
        }
        else if( !haveIncrementedChordTones[0] && 
            SunbeamInteractors.sunbeamAccumulated - sunbeamAmountAtSecondHalf > 5 )
        {
            chord2[0] += 12;
            myChuck.SetFloatArray( myChordNotesVar, chord2 );
            haveIncrementedChordTones[0] = true;
        }
    }

    private void TurnOn()
    {
        iAmPlaying = true;
        TheChuck.instance.SetFloat( "bassNote", secondHalf ? bassNote2 : bassNote1 );
        myChuck.SetFloatArray( myChordNotesVar, secondHalf ? chord2 : chord1 );
    }

    public void TurnOff()
    {
        if( !iAmPlaying ) return;

        iAmPlaying = false;
    }

    private float sunbeamAmountAtSecondHalf;
    private bool[] haveIncrementedChordTones = { false, false, false, false };
    public void AdvanceToSecondHalf()
    {
        secondHalf = true;
        myChuck.SetFloatArray( myChordNotesVar, chord2 );
        sunbeamAmountAtSecondHalf = SunbeamInteractors.sunbeamAccumulated;
    }

    private string myChordNotesVar, myGainVar;
    private ChuckEventListener myRegularTatum;
    private void StartChuck()
    {
        myChordNotesVar = myChuck.GetUniqueVariableName();
        myGainVar = myChuck.GetUniqueVariableName();
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
            2000 => lpf.freq; // orig 6000
            200 => hpf.freq;
            0.05 => reverb.mix;
            0.15 => reverb.gain;

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

            69 => int A4;
            71 => int B4;
            74 => int D5;
            76 => int E5;
            78 => int Fs5;
            79 => int G5;


            for( int i; i < 4; i++ )
            {{
                // how much freq waves up and down as a % of current freq
                1.2 / 300 => mySaws[i].pitchLFODepthMultiplier; 
                // how quickly the freq lfo moves
                3.67 => mySaws[i].pitchLFORate;
                // higher == wider pitch spread
                0.33 => mySaws[i].timbreLFO; 
                // basic loudness
                0.035 => mySaws[i].gain;
    
                mySaws[i] => hpf;
            }}

            [59.0, 62, 66, 69] @=> global float {0}[];
            [59.0, 62, 66, 69] @=> float currentChordNotes[];
            [0.005, 0.0046, 0.0044, 0.0041] @=> float chordSlews[];
            0 => global float {1};
            0 => float currentGain;
            fun void SlewChordFreqs()
            {{
                while( true )
                {{
                    currentGain + chordSlews[0] * ({1} - currentGain) => currentGain;
                    for( int i; i < 4; i++ )
                    {{
                        currentChordNotes[i] + 3 * chordSlews[i] * 
                            ( {0}[i] - currentChordNotes[i] ) => currentChordNotes[i];
                        currentChordNotes[i] - 0 => Std.mtof => mySaws[i].freq;
                        currentGain => mySaws[i].gain;
                    }}
                    1::ms => now;
                }}
            }}
            spork ~ SlewChordFreqs();

            fun void SetLPFCutoff()
            {{
                global float scene2TransitionProgress;
                while( true )
                {{
                    1200 + 4000 * scene2TransitionProgress => lpf.freq;
                    10::ms => now;
//if( Math.random2f( 0, 1 ) < 0.005 ) {{ <<< scene2TransitionProgress >>>; }}
                }}
            }}
            spork ~ SetLPFCutoff();

            while( true ) {{ 1::second => now; }}
        ", myChordNotesVar, myGainVar ) );
        

        myChuck.RunCode( @"
            0.12::second => dur tatum;
            global Event scene2Regular16Tatum;
            while( true )
            {
                scene2Regular16Tatum.broadcast();
                16::tatum => now;
            }
        " );
        myRegularTatum = gameObject.AddComponent<ChuckEventListener>();
        myRegularTatum.ListenForEvent( myChuck, "scene2Regular16Tatum", UpdateChordTones );
    }
}
