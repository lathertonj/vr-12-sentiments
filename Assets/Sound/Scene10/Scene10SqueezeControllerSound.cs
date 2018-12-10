using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene10SqueezeControllerSound : MonoBehaviour
{
    ChuckSubInstance myChuck;
    private string mySqueezeEvent, myUnsqueezeEvent;
    public string[] myChord0, myChord1, myChord2, myChord3;

	private ControllerAccessors myController;

    // Use this for initialization
    void Start()
    {
		myController = GetComponent<ControllerAccessors>();
        myChuck = GetComponent<ChuckSubInstance>();
		mySqueezeEvent = myChuck.GetUniqueVariableName();
		myUnsqueezeEvent = myChuck.GetUniqueVariableName();

        // synth chords
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
			ADSR adsr => hpf; // should be: hpf
            15000 => lpf.freq; // orig 2000
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

			[{0}] @=> global int myNotes[];
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
                0.07 => mySaws[i].gain; // should be: 0.035
    
				// pitch
				myNotes[i] => Std.mtof => mySaws[i].freq;

                mySaws[i] => adsr;
            }}

            float currentChordNotes[myNotes.size()];
            for( int i; i < currentChordNotes.size(); i++ )
            {{
                myNotes[i] => currentChordNotes[i];
            }}
            0.5 => float chordSlew;
            fun void SlewChordFreqs()
            {{
                while( true )
                {{
                    for( int i; i < mySaws.size(); i++ )
                    {{
                        currentChordNotes[i] + chordSlew * 
                            ( myNotes[i] - currentChordNotes[i] ) => currentChordNotes[i];
                        currentChordNotes[i] - 0 => Std.mtof => mySaws[i].freq;
                    }}
                    1::ms => now;
                }}
            }}
            spork ~ SlewChordFreqs();

            global float scene3TransitionProgress;
            fun void SetLPFCutoff()
            {{
                while( true )
                {{
                    250 - 230 * scene3TransitionProgress => hpf.freq;
                    10::ms => now;
                }}
            }}
            spork ~ SetLPFCutoff();


            global Event {1}, {2};
			global float scene10NoteLengthSeconds;

			fun void PlayNotes()
			{{
				while( true )
				{{
					me.yield();
					1 => adsr.keyOn;
					scene10NoteLengthSeconds::second => now;

					1 => adsr.keyOff;
					scene10NoteLengthSeconds::second => now;
				}}
			}}
            
            fun void RespondToChordEvents()
            {{
                while( true ) {{
                    {1} => now;
					scene10NoteLengthSeconds::second => dur T;
					// sync
					T - ( now % T ) => now;
					// play
                    spork ~ PlayNotes() @=> Shred playNotesShred;
                    {2} => now;
					// turn off
					playNotesShred.exit();
                    1 => adsr.keyOff;
                }}
            }}
            spork ~ RespondToChordEvents();

            global Event scene3FadeOut;
            scene3FadeOut => now;
            
            while( true )
            {{
                adsr.gain() * 0.99 => adsr.gain;
                10::ms => now;
            }}
        ", string.Join( ",", myChord0 ), mySqueezeEvent, myUnsqueezeEvent ) );

        // shakers and create modey
        myChuck.RunCode( string.Format( @"
            global float scene10NoteLengthSeconds;
            global Event {0}, {1};
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
                    Math.random2f( 0.7, 0.9 ) => s.energy;
                    1 => s.noteOn;
                    scene10NoteLengthSeconds::second => now;

                    me.yield();
                    Math.random2f( 0.1, 0.3 ) => s.energy;
                    1 => s.noteOn;
                    scene10NoteLengthSeconds::second => now;
    
                    me.yield();
                    Math.random2f( 0.3, 0.5 ) => s.energy;
                    1 => s.noteOn;
                    scene10NoteLengthSeconds::second => now;

                    me.yield();
                    Math.random2f( 0.1, 0.3 ) => s.energy;
                    1 => s.noteOn;
                    scene10NoteLengthSeconds::second => now;
                }}

            }}

            fun void RespondToSqueezeEvents()
            {{
                while( true )
                {{
                    {0} => now;
					scene10NoteLengthSeconds::second => dur T;
					// sync
					T - ( now % T ) => now;
					// play
                    spork ~ Play() @=> Shred playShred;
                    {1} => now;
                    playShred.exit();
                }}
            }}
            spork ~ RespondToSqueezeEvents();

            global Event scene3FadeOut;
            scene3FadeOut => now;
            
            // mute shakers
            while( true )
            {{
                s.gain() * 0.99 => s.gain;
                10::ms => now;
            }}
            
            
        ", mySqueezeEvent, myUnsqueezeEvent ) );
    }

    // Update is called once per frame
    void Update()
    {
		if( myController.IsFirstSqueezed() )
		{
			myChuck.BroadcastEvent( mySqueezeEvent );
		}

		if( myController.IsUnSqueezed() )
		{
			myChuck.BroadcastEvent( myUnsqueezeEvent );
		}
    }
}
