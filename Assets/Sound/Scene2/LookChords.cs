using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookChords : MonoBehaviour
{
    public double[] chord1Notes, chord2Notes, chord3Notes, chord4Notes;
    public float secondsToChordChange = 1;
    private int currentChord = 0;

    // TODO:
    // - I am not satisfied with the slewing behavior to switch between chords
    // -- I made it 3x faster and now I'm a little more satisfied
    // - Need to program the light things to fade in and out and change where they are slowly
    // - Need to make a leaf model for your hands
    // - As things get more progressed, open up filters and make the bass louder to compensate
    // - As things get more progressed, make the lowest note one octave higher
    // - The other modalbar thing (and progress it too)

    // Use this for initialization
    void Start()
    {
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
            //        Math.random2f( -0.1, 0.1) => baseFreqs[i];
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

            HPF hpf => LPF lpf => Gain ampMod => NRev reverb => dac;
            1800 => hpf.freq;
            2000 => lpf.freq; // orig 6000
            200 => hpf.freq;
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

            Supersaw mySaw => ADSR myAdsr => hpf;
            myAdsr.set( 0.1::noteLength, 0.3::noteLength, 0.85, 0.05::noteLength );


            // how much freq waves up and down as a % of current freq
            1.2 / 300 => mySaw.pitchLFODepthMultiplier; 
            // how quickly the freq lfo moves
            3.67 => mySaw.pitchLFORate;
            // higher == wider pitch spread
            0.33 => mySaw.timbreLFO; 
            // basic loudness
            0.05 => mySaw.gain;


            [59.0, 62, 66, 69] @=> global float chordNotes[];
            [59.0, 62, 66, 69] @=> float currentChordNotes[];
            [0.005, 0.0046, 0.0044, 0.0041] @=> float chordSlews[];
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
            [59.0, 54.0, 50.0, 52.0 ] @=> float bassNotes[];
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


// TODO remove this short circuit
mySaw.gain( 0 );
while( true )
{
    1::second => now;
}


            [G5, Fs5, Fs5, G5] @=> global int melodyNotes[];

            while( true )
            {
                for( int i; i < melodyNotes.size(); i++ )
                {
                    melodyNotes[i] => Std.mtof => mySaw.freq;
                    myAdsr.keyOn( true );
                    noteLength - myAdsr.releaseTime() => now;
                    myAdsr.keyOff( true );
                    myAdsr.releaseTime() => now;
                }
            }
    " );
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
            if( !hit.collider.gameObject.CompareTag( currentHitType ) )
            {
                currentHitType = hit.collider.gameObject.tag;
                timeOfPreviousSwitch = Time.time;
                switch( currentHitType )
                {
                    case "Chord1":
                        nextChord = secondSetOfChords ? 3 : 1;
                        break;
                    case "Chord2":
                        nextChord = secondSetOfChords ? 4 : 2;
                        break;
                }
            }
        }

        if( nextChord != currentChord )
        {
            float elapsedTime = Time.time - timeOfPreviousSwitch;
            if( elapsedTime > secondsToChordChange )
            {
                // change the chord!!
                currentChord = nextChord;
                switch( currentChord )
                {
                    case 1:
                        // TODO set melody?
                        TheChuck.instance.SetFloatArray( "chordNotes", chord1Notes );
                        TheChuck.instance.SetInt( "bassNote", 1 );
                        break;
                    case 2:
                        // TODO set melody?
                        TheChuck.instance.SetFloatArray( "chordNotes", chord2Notes );
                        TheChuck.instance.SetInt( "bassNote", 2 );
                        break;
                    case 3:
                        // TODO set melody?
                        TheChuck.instance.SetFloatArray( "chordNotes", chord3Notes );
                        TheChuck.instance.SetInt( "bassNote", 3 );
                        break;
                    case 4:
                        // TODO set melody?
                        TheChuck.instance.SetFloatArray( "chordNotes", chord4Notes );
                        TheChuck.instance.SetInt( "bassNote", 4 );
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
}
