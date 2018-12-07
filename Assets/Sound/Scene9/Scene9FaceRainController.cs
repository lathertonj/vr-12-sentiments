using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Scene9FaceRainController : MonoBehaviour
{

    public ControllerAccessors[] myControllers;

    private SphereCollider myCollider;
    public float colliderBaseRadius = 0.2f;

    private ChuckSubInstance myChuck;
    public long[] myModalNotes;
    public string[] myChordNotes;
    private string myNotesVar;
    private string myGoalGainVar;
    private string myPlayNoteVar;

    private float numRecentHits = 0;
    private int numTotalHits = 0;
    public int numHitsToFinish = 100;

    // Use this for initialization
    void Start()
    {
        myCollider = GetComponent<SphereCollider>();
        myChuck = GetComponent<ChuckSubInstance>();
        myNotesVar = myChuck.GetUniqueVariableName();
        myPlayNoteVar = myChuck.GetUniqueVariableName();
        myGoalGainVar = myChuck.GetUniqueVariableName();

		// start with some sound
		numRecentHits = 1.5f;

        // =================================================================
        // =                          MODALBAR                             =
        // =================================================================
        myChuck.RunCode( string.Format( @"
			global int {0}[1];
            global Event {1};
			global ModalBar modey => JCRev reverb => dac;
            0.15::second => dur minNoteLength;

            true => int playStrong;

            fun void PlayNotes()
            {{
                int i, prevI;
                while( true )
                {{
					// wait until told to play next note
					{1} => now;

                    // select next note index
                    while( i == prevI && {0}.size() > 1 )
                    {{
                        Math.random2( 0, {0}.size() - 1 ) => i;
                        me.yield();
                    }}
                    i => prevI;

					// play note
					if( {0}[i] > 10 )
					{{
						Math.random2f( 0.2, 0.8 ) => modey.strikePosition;
						{0}[i] => Std.mtof => modey.freq;

						me.yield();

						Math.random2f( 0.1, 0.2 ) + playStrong * 0.05 => modey.strike;
						!playStrong => playStrong;
					}}

					// wait for next note
					minNoteLength => now;
                }}
            }}
			spork ~ PlayNotes() @=> Shred playNotesShred;

			global Event scene9EndEvent;
			scene9EndEvent => now;

			playNotesShred.exit();

			10::second => now;
		
		", myNotesVar, myPlayNoteVar ) );
        myChuck.SetIntArray( myNotesVar, myModalNotes );


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
            1000 => lpf.freq; // orig 6000
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

            69 => int A4;
            73 => int Cs5;
            76 => int E5;
            83 => int B5;
            90 => int Fs5;

            [{0}] @=> int myNotes[];
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

			global float {1};
            0 => float goalGain;
            0 => float currentGain;
            0.0006 => float gainSlew;
			true => int respondToGain;
            fun void ApplyGain()
            {{
                while( true )
                {{
					if( respondToGain )
					{{
						{1} => goalGain;
					}}
                    gainSlew * ( goalGain - currentGain ) +=> currentGain;
                    currentGain => lpf.gain;
                    500 + 5000 * currentGain => lpf.freq;
                    1::ms => now;
                }}
            }}
            spork ~ ApplyGain();

            
            // TODO:
            // turn off chord at end of movement
            global Event scene9EndEvent;
            scene9EndEvent => now;
            //squeezeResponseShred.exit();
			false => respondToGain;
            0 => goalGain;
            
            // let it die out
            while( true ) 
            {{ 
                0 => goalGain;
                1::second => now;
            }} 
        ", string.Join( ",", myChordNotes ), myGoalGainVar ) );
    }

    void Update()
    {
        // decay
        numRecentHits *= 0.995f;

        // sound
        myChuck.SetFloat( myGoalGainVar, numRecentHits.MapClamp( 0, 5, 0, 1 ) );

        // size of collider
        float multiplier = 1;
        foreach( ControllerAccessors controller in myControllers )
        {
            multiplier *= ControllerHeight( controller.transform ).PowMapClamp( 0, 0.8f, 1, 2, pow: 1 );
        }
        float radius = colliderBaseRadius * multiplier;
        myCollider.radius = radius;
        myCollider.center = -0.75f * radius * Vector3.up;
    }

    private float ControllerHeight( Transform controller )
    {
        return controller.position.y - transform.position.y;
    }

    private bool sceneIsOver = false;

    public void InformHit()
    {
		// ignore
        if( sceneIsOver ) { return; }

        // remember
        numRecentHits += 1;
		numTotalHits += 1;

        // vibrate
        foreach( ControllerAccessors controller in myControllers )
        {
            if( ControllerHeight( controller.transform ) > 0 )
            {
                controller.Vibrate( 500 );
            }
            float vibrateIntensity = ControllerHeight( controller.transform ).PowMapClamp( 0, 0.8f, 0, 500, pow: 2 );
        }

        // sound
        myChuck.BroadcastEvent( myPlayNoteVar );

        if( !sceneIsOver && numTotalHits >= numHitsToFinish )
        {
            sceneIsOver = true;

			// mute audio
			myChuck.BroadcastEvent( "scene9EndEvent" );

			// fade visuals
			Invoke( "EndScene", 4f );
        }
    }

	private void EndScene()
	{
		SteamVR_Fade.Start( GetComponent<FadeInScene>().skyColor, duration: 5f );
		Invoke( "SwitchToNextScene", 8f );
	}

	private void SwitchToNextScene()
	{
		SceneManager.LoadScene( "10_Frenetic_11_SublimeEmptiness" );
	}
}
