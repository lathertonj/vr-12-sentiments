﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene5LookChords : MonoBehaviour
{
    void Start()
    {
        currentCutoffs = new float[4];
        TheChuck.instance.RunCode( @"
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
            {
                SawOsc osc => Gain out => outlet;
                5 => int numDelays;
                1.0 / (1 + numDelays) => out.gain;
                DelayA theDelays[numDelays];
                SinOsc lfos[numDelays];
                dur baseDelays[numDelays];
                float baseFreqs[numDelays];
                for( int i; i < numDelays; i++ )
                {
                    osc => theDelays[i] => out;
                    0.15::second => theDelays[i].max;
                    // crucial to modify!
                    Math.random2f( 0.001, 0.002 )::second => baseDelays[i];
                    lfos[i] => blackhole;
                    Math.random2f( 0, pi ) => lfos[i].phase;
        
                }
                0.05::second => dur baseDelay;
                0.333 => float baseFreq;
                1 => float lfoGain;
    
                fun void AttachLFOs()
                {
                    while( true )
                    {
                        for( int i; i < numDelays; i++ )
                        {
                            baseFreq + baseFreqs[i] => lfos[i].freq;
                            lfoGain * lfos[i].last()::second + baseDelay + baseDelays[i] 
                                => theDelays[i].delay;
                        }
                        1::ms => now;
                    }
                }
                spork ~ AttachLFOs();
    
    
                SinOsc pitchLFO => blackhole;
                0.77 => pitchLFO.freq;
                1.0 / 300 => float pitchLFODepth;
                440 => float basePitch;
    
                fun void FreqMod()
                {
                    while( true )
                    {
                        // calc freq
                        basePitch + ( basePitch * pitchLFODepth ) 
                            * pitchLFO.last() => float f;
                        // set
                        f => osc.freq;
                        1.0 / f => lfoGain; // seconds per cycle == gain amount
                        // wait
                        1::ms => now;
                    }
                }
                spork ~ FreqMod();
    
                fun void freq( float f )
                {
                    f => basePitch;
                }
    
                fun void delay( dur d )
                {
                    d => baseDelay;
                }
    
                fun void timbreLFO( float f )
                {
                    f => baseFreq;
                }
    
                fun void pitchLFORate( float f )
                {
                    f => pitchLFO.freq;
                }
    
                fun void pitchLFODepthMultiplier( float r )
                {
                    r => pitchLFODepth;
                }
            }

            HPF hpf => LPF lpf => Gain ampMod => PRCRev reverb => dac;
            20 => hpf.freq; // UNUSED
            2000 => lpf.freq; // orig 6000
            0.05 => reverb.mix;

            fun void AmpMod()
            {
                SinOsc lfo => blackhole;
                0.13 => lfo.freq;
                while( true )
                {
                     0.85 + 0.15 * lfo.last() => ampMod.gain;
                     1::ms => now;
                }
            }
            spork ~ AmpMod();

            // TODO: this could use some amplitude modulation
            Supersaw sawChord1[5];
            Supersaw sawChord2[4];

            Gain sawChordOut1 => lpf;
            Gain sawChordOut2 => lpf;

            64 => int E4;
            66 => int Fs4;
            68 => int Gs4;
            69 => int A4;
            71 => int B4;
            73 => int Cs5;
            76 => int E5;
            78 => int Fs5;
            83 => int B5;

            [Fs4 - 24, Cs5 - 12, Fs4, A4,  Cs5, E5, B5] @=> int chord1Notes[];
            [E4 - 24,  B4 - 12,  E4,  Gs4, B4,  Fs5] @=> int chord2Notes[];


            for( int i; i < sawChord1.size(); i++ )
            {
                // how much freq waves up and down as a % of current freq
                1.2 / 300 => sawChord1[i].pitchLFODepthMultiplier; 
                // how quickly the freq lfo moves
                3.67 => sawChord1[i].pitchLFORate;
                // higher == wider pitch spread
                0.33 => sawChord1[i].timbreLFO; 
                // basic loudness
                0.035 => sawChord1[i].gain;
                // pitch
                chord1Notes[i] => Std.mtof => sawChord1[i].freq;
    
                sawChord1[i] => sawChordOut1;
            }

            for( int i; i < sawChord2.size(); i++ )
            {
                // how much freq waves up and down as a % of current freq
                1.2 / 300 => sawChord2[i].pitchLFODepthMultiplier; 
                // how quickly the freq lfo moves
                3.67 => sawChord2[i].pitchLFORate;
                // higher == wider pitch spread
                0.33 => sawChord2[i].timbreLFO; 
                // basic loudness
                0.035 => sawChord2[i].gain;
                // pitch
                chord2Notes[i] => Std.mtof => sawChord2[i].freq;
    
                sawChord2[i] => sawChordOut2;
            }

            global float scene5LookAmount;
            float currentScene5LookAmount;
            0.001 => float slewScene5LookAmount;
            50 => float lpfLowCutoff;
            1000 - lpfLowCutoff => float lpfHighCutoff;
            
            fun void SetSound()
            {
                while( true )
                {
                    slewScene5LookAmount * ( scene5LookAmount - currentScene5LookAmount ) +=> currentScene5LookAmount;
                    currentScene5LookAmount => float a;
                    if( a <= 0.001 && a >= -0.001 )
                    {
                        // turn everything off
                        0 => sawChordOut1.gain;
                        0 => sawChordOut2.gain;
                    }
                    else if( a > 0 )
                    {
                        a => float amount;
                        Math.min( amount, 1 ) => amount;
                        Math.sqrt( amount ) => sawChordOut1.gain;
                        lpfLowCutoff + lpfHighCutoff * amount => lpf.freq;                        
                    }
                    else if( a < 0 )
                    {
                        a * -1 => float amount;
                        Math.min( amount, 1 ) => amount;
                        Math.sqrt( amount ) => sawChordOut2.gain;
                        lpfLowCutoff + lpfHighCutoff * amount => lpf.freq;
                    }
    
                    1::ms => now;
                }
            }
            spork ~ SetSound();
            
            while( true ) { 1::second => now; }
            
    " );
    }

    // Update is called once per frame
    public float[] startCutoffs;
    public float[] endCutoffs;
    public float cutoffTransitionStartTime;
    public float cutoffTransitionTime;
    private float[] currentCutoffs;
    void Update()
    {
        float angle = transform.localEulerAngles.x;
        if( angle > 180 ) { angle -= 360; }

        float normElapsedTime = Mathf.Clamp01( ( Time.time - cutoffTransitionStartTime ) / cutoffTransitionTime );
        for( int i = 0; i < currentCutoffs.Length; i++ )
        {
            currentCutoffs[i] = startCutoffs[i] + normElapsedTime * ( endCutoffs[i] - startCutoffs[i] );
        }

        float amount = 0;
        if( angle < currentCutoffs[1] )
        {
            amount = -1 * angle.MapClamp( currentCutoffs[1], currentCutoffs[0], 0, 1 );
        }
        else if( angle > currentCutoffs[2] )
        {
            amount = angle.MapClamp( currentCutoffs[2], currentCutoffs[3], 0, 1 );
        }


        TheChuck.instance.SetFloat( "scene5LookAmount", amount );

        if( Time.time > cutoffTransitionStartTime + cutoffTransitionTime )
        {
            // TODO end scene
            // turn everything to 0 (do it by just increasing slew then setting amount to 0)
            // have a small swell of negative chord (do it by waiting, then just setting amount to 0.3)
            // then turn everything back to 0 and fade scene (do it by waiting, then just setting amount to 0)
        }
    }
}
