using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene7SqueezeHandSonifier : MonoBehaviour
{

    public int[] myChord;
    private ChuckSubInstance myChuck;
	private string mySqueezedEvent, myUnsqueezedEvent;
	private ControllerAccessors myController;
    // Use this for initialization
    void Start()
    {
        myChuck = GetComponent<ChuckSubInstance>();
		myController = GetComponent<ControllerAccessors>();
		mySqueezedEvent = myChuck.GetUniqueVariableName();
		myUnsqueezedEvent = myChuck.GetUniqueVariableName();

		string myChordString = "[";
		for( int i = 0; i < myChord.Length; i++ )
		{
			myChordString += myChord[i].ToString();
			if ( i != myChord.Length - 1 )
			{
				myChordString += ",";
			}
		}
		myChordString += "]";

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
            400 => hpf.freq;
            16000 => lpf.freq; // orig 6000
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

            {2} @=> int myNotes[];
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

            0 => float goalGain;
            0 => float currentGain;
            0.0006 => float gainSlew;
            fun void ApplyGain()
            {{
                while( true )
                {{
                    gainSlew * ( goalGain - currentGain ) +=> currentGain;
                    currentGain * 1.3 => lpf.gain;
                    400 + 8000 * currentGain => lpf.freq;
                    1::ms => now;
                }}
            }}
            spork ~ ApplyGain();

            global Event {0}, {1};
            fun void RespondToSqueezes()
            {{
                while( true )
                {{
                    {0} => now;
                    1 => goalGain;
                    {1} => now;
                    0 => goalGain;
                }}
            }}
            spork ~ RespondToSqueezes() @=> Shred squeezeResponseShred;
            
			while( true ) {{ 1::second => now; }}
            // turn off chord at end of movement

            squeezeResponseShred.exit();
            0 => goalGain;
            
            // let it die out
            while( true ) 
            {{ 
                0 => goalGain;
                1::second => now;
            }} 
        ", mySqueezedEvent, myUnsqueezedEvent, myChordString ) );
    }

    // Update is called once per frame
    void Update()
    {
		if( myController.IsFirstSqueezed() )
		{
			myChuck.BroadcastEvent( mySqueezedEvent );
		}
		
		if( myController.IsUnSqueezed() )
		{
			myChuck.BroadcastEvent( myUnsqueezedEvent );
		}
    }
}
