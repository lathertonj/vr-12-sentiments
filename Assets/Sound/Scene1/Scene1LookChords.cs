using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene1LookChords : MonoBehaviour
{
    public double[] chord1Notes, chord2Notes, chord3Notes, chord4Notes;
    public float secondsToChordChange = 1;
    private int currentChord = 0;
    public Collider chord1Plane, chord2Plane;
    private ChuckSubInstance myChuck;
    private ChuckFloatSyncer myCurrentLoudnessSyncer;

    // Use this for initialization
    void Start()
    {
        myChuck = GetComponent<ChuckSubInstance>();
        myChuck.RunCode( @"
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

            HPF hpf => LPF lpf => Gain ampMod => JCRev reverb => Gain volumeControl => dac;
            1800 => hpf.freq;
            2000 => lpf.freq; // orig 6000
            200 => hpf.freq;
            0.05 => reverb.mix;
            0 => volumeControl.gain; // off by default

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
            Supersaw mySaws[4];

            69 => int A4;
            71 => int B4;
            74 => int D5;
            76 => int E5;
            78 => int Fs5;
            79 => int G5;


            for( int i; i < 4; i++ )
            {
                // how much freq waves up and down as a % of current freq
                1.2 / 300 => mySaws[i].pitchLFODepthMultiplier; 
                // how quickly the freq lfo moves
                3.67 => mySaws[i].pitchLFORate;
                // higher == wider pitch spread
                0.33 => mySaws[i].timbreLFO; 
                // basic loudness
                0.035 => mySaws[i].gain;
    
                mySaws[i] => hpf;
            }

            0.12::second * 2 => dur noteLength;


            [59.0, 62, 66, 69] @=> global float chordNotes[];
            [59.0, 62, 66, 69] @=> float currentChordNotes[];
            [0.002, 0.0016, 0.0014, 0.0011] @=> float chordSlews[];
            [0.005, 0.0046, 0.0044, 0.0041] @=> chordSlews;
            [0.0035, 0.0033, 0.0030, 0.0029] @=> chordSlews;
            fun void SlewChordFreqs()
            {
                while( true )
                {
                    for( int i; i < 4; i++ )
                    {
                        currentChordNotes[i] + 3 * chordSlews[i] * 
                            ( chordNotes[i] - currentChordNotes[i] ) => currentChordNotes[i];
                        currentChordNotes[i] - 0 => Std.mtof => mySaws[i].freq;
                    }
                    1::ms => now;
                }
            }
            spork ~ SlewChordFreqs();

            SinOsc bass => reverb;
            0.04 => bass.gain;
            [59.0, 54.0, 55.0, 50.0 ] @=> float bassNotes[];
            12 => int bassDownwardOffset;
            1 => global int bassNote;

            fun void SlewBass() 
            {
                bassNotes[bassNote - 1] => float currentBassNote; 
                0.05 => float bassSlew;

                while( true )
                {
                    currentBassNote + 3 * bassSlew * ( bassNotes[bassNote - 1] - currentBassNote ) => currentBassNote;
                    currentBassNote - bassDownwardOffset => Std.mtof => bass.freq;
                    1::ms => now;
                }
            }
            spork ~ SlewBass();

            fun void SetVolumeLevel( float level )
            {
                volumeControl.gain() => float startLevel;
                1000 => int numSteps;
                dur changeTime;
                if( startLevel < level )
                {
                    3::second => changeTime;
                }
                else
                {
                    1.7::second => changeTime;
                }
                for( int i; i < numSteps; i++ )
                {
                    startLevel + i * 1.0 / numSteps * ( level - startLevel ) => volumeControl.gain; 
                    changeTime / numSteps => now;
                }   
            }

            global float scene1CurrentLoudness;
            fun void SetCurrentLoudness()
            {
                while( true )
                {
                    volumeControl.gain() => scene1CurrentLoudness;
                    1::ms => now;
                }
            }
            spork ~ SetCurrentLoudness();

            global float scene1GoalLoudness;
            float currentLoudness;
            float scene1LoudnessSlew;

            global Event endScene1;
            fun void ListenForEnd()
            {
                endScene1 => now;
                0.0003 => scene1LoudnessSlew;
                0 => scene1GoalLoudness;
            }
            spork ~ ListenForEnd();

            while( true )
            {
                if( currentLoudness < scene1GoalLoudness )
                {
                    0.0003 => scene1LoudnessSlew; // ascend slowly
                }
                else
                {
                    0.002 => scene1LoudnessSlew; // descend quickly
                }
                
                scene1LoudnessSlew * ( scene1GoalLoudness - currentLoudness ) +=> currentLoudness;
                currentLoudness => volumeControl.gain;
                1::ms => now;
            }
            
        " );
        myCurrentLoudnessSyncer = gameObject.AddComponent<ChuckFloatSyncer>();
        myCurrentLoudnessSyncer.SyncFloat( myChuck, "scene1CurrentLoudness" );

    }

    static private bool shouldUpdateChuck = true;
    static public void EndSceneAudio()
    {
        shouldUpdateChuck = false;
        TheChuck.instance.BroadcastEvent( "endScene1" );
    }

    // Update is called once per frame
    string currentHitType;
    float timeOfPreviousSwitch;
    int nextChord = 0;
    void Update()
    {
        RaycastHit hit;
        int layerMask = LayerMask.GetMask( "LookChordPlane" );
        if( Physics.Raycast( transform.position, transform.forward, out hit, Mathf.Infinity, layerMask ) )
        {
            if( hit.collider == chord1Plane )
            {
                nextChord = secondSetOfChords ? 3 : 1;
            }
            else if( hit.collider == chord2Plane )
            {
                nextChord = secondSetOfChords ? 4 : 2;
            }
            else
            {
                nextChord = 0;
            }
            
        }
        else
        {
            nextChord = 0;
        }

        if( nextChord != currentChord && shouldUpdateChuck )
        {
            float elapsedTime = Time.time - timeOfPreviousSwitch;
            if( elapsedTime > secondsToChordChange )
            {
                // change the chord!!
                currentChord = nextChord;
                timeOfPreviousSwitch = Time.time;
                switch( currentChord )
                {
                    case 0:
                        // TURN THINGS OFF
                        myChuck.SetFloat( "scene1GoalLoudness", 0 );
                        break;
                    case 1:
                        myChuck.SetFloatArray( "chordNotes", chord1Notes );
                        myChuck.SetInt( "bassNote", 1 );
                        myChuck.SetFloat( "scene1GoalLoudness", 1 );
                        break;
                    case 2:
                        myChuck.SetFloatArray( "chordNotes", chord2Notes );
                        myChuck.SetInt( "bassNote", 2 );
                        myChuck.SetFloat( "scene1GoalLoudness", 1 );
                        break;
                    case 3:
                        myChuck.SetFloatArray( "chordNotes", chord3Notes );
                        myChuck.SetInt( "bassNote", 3 );
                        myChuck.SetFloat( "scene1GoalLoudness", 1 );
                        break;
                    case 4:
                        myChuck.SetFloatArray( "chordNotes", chord4Notes );
                        myChuck.SetInt( "bassNote", 4 );
                        myChuck.SetFloat( "scene1GoalLoudness", 1 );
                        break;
                }
                
            }
        }
    }

    private bool secondSetOfChords = false;
    public void SwitchToSecondSetOfChords()
    {
        secondSetOfChords = true;
    }

    public float GetCurrentLoudness()
    {
        return myCurrentLoudnessSyncer.GetCurrentValue();
    }
}
