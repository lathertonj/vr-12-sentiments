using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene3Advancer : MonoBehaviour
{
    public FlowerAddSeedlings2 leftHand, rightHand;
    public Color skyColor;

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    bool haveAdvanced = false;
    void Update()
    {
        if( !haveAdvanced && leftHand.numLongPresses >= 2 && rightHand.numLongPresses >= 2 )
        {
            haveAdvanced = true;
            // advance!
            float period = 0.17f;
            float timeToStart = 0;
            leftHand.StartSpewing( timeToStart, period );
            rightHand.StartSpewing( timeToStart + period / 2, period );
            AdvanceChuck();
            Invoke( "DoCameraFade", time: 15f );
        }
    }

    void AdvanceChuck()
    {
        // things that only happen once should be in here
        // advance TransitionProgress
        TheChuck.instance.RunCode( @"
            0 => global float scene3TransitionProgress;

            5::second => dur rampTime;
            1000 => int numSteps;
            for( int i; i < numSteps; i++ )
            {
                1.0 / numSteps +=> scene3TransitionProgress;
                rampTime / numSteps => now;
            }
        " );

        // add a melody line
        TheChuck.instance.RunCode( string.Format( @"
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
            ADSR adsr => hpf; // should be: hpf
            15000 => lpf.freq; // orig 2000
            20 => hpf.freq; // possibly 1800
            0.05 => reverb.mix;
            0.85 => reverb.gain;
            adsr.set( 0.01::second, 0.02::second, 0.8, 0.01::second );

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

            Supersaw mySaw => adsr;

            1.2 / 300 => mySaw.pitchLFODepthMultiplier; 
            // how quickly the freq lfo moves
            3.67 => mySaw.pitchLFORate;
            // higher == wider pitch spread
            0.66 => mySaw.timbreLFO; // orig .33
            // basic loudness
            0 => mySaw.gain;

            0.08 => float goalGain;
            0 => float currentGain;
            0.001 => float gainSlew;

            fun void SlewGain()
            {{
                while( true )
                {{
                    currentGain + gainSlew * ( goalGain - currentGain ) => currentGain;
                    currentGain => mySaw.gain;
                    7::ms => now;
                }}
            }}
            spork ~ SlewGain();

            0.22::second => dur noteLength;
            [[81, 80, 80, 81],
             [83, 81, 81, 83],
             [85, 83, 83, 85]] @=> int myNotes[][];

            [20, 10, 100] @=> int numRepetitions[];

            // TODO: I am not sure I like the above, revert back to original behavior like this:
            [[81, 80, 80, 81]] @=> myNotes;
            [1] @=> numRepetitions;

            fun void PlayMelodyNotes()
            {{
                while( true )
                {{
                    for( int j; j < myNotes.size(); j++ )
                    {{
                        repeat( numRepetitions[j] ) 
                        {{  
                            for( int i; i < myNotes[j].size(); i++ )
                            {{
                                myNotes[j][i] => Std.mtof => mySaw.freq;
                                1 => adsr.keyOn;
                                noteLength - adsr.releaseTime() => now;
                                1 => adsr.keyOff;
                                adsr.releaseTime() => now;
                            }}
                        }}
                    }}
                }}
            }}
            spork ~ PlayMelodyNotes();
            
            global Event scene3FadeOut;
            scene3FadeOut => now;
            while( true )
            {{
                adsr.gain() * 0.99 => adsr.gain;
                10::ms => now;
            }}
            
        " ) );
    }

    void DoCameraFade()
    {
        SteamVR_Fade.Start( skyColor, duration: 8f, fadeOverlay: true );
        Invoke( "DoAudioFade", 2.5f );
        Invoke( "SwitchToNextScene", 11f );
    }

    void DoAudioFade()
    {
        TheChuck.instance.BroadcastEvent( "scene3FadeOut" );
        // also fade vibration
        FlowerAddSeedlings2.decreaseVibrationIntensity = true;
    }

    void SwitchToNextScene()
    {
        SceneManager.LoadScene( "4_FlowingLightness" );
    }
}
