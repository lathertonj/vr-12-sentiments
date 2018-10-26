using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( ChuckSubInstance ) )]
public class Scene4SonifyFlowerSeedlings : MonoBehaviour
{
    private ChuckSubInstance myChuck;
    public double[] myChord;
    public double[] myArpeggio;

    private string myChordNotesVar, myArpeggioNotesVar, myModey;
    private string mySqueezedEvent, myUnsqueezedEvent;
    private string myJumpEvent;
    public void StartChuck( float jumpDelay, System.Action launchASeedling )
    {
        myChuck = GetComponent<ChuckSubInstance>();
        myChordNotesVar = myChuck.GetUniqueVariableName();
        myArpeggioNotesVar = myChuck.GetUniqueVariableName();
        myModey = myChuck.GetUniqueVariableName();
        myJumpEvent = myChuck.GetUniqueVariableName();
        mySqueezedEvent = myChuck.GetUniqueVariableName();
        myUnsqueezedEvent = myChuck.GetUniqueVariableName();


        // respond to individual notes
        myChuck.RunCode( string.Format( @"
            global ModalBar {0} => JCRev reverb => dac;
            global float {1}[1];
            global Event {2}, {3}, {5};
            {4}::second => dur noteLength;
            0.08::second => dur jumpDelay;

            true => int playStrong;

            fun void PlayNotes()
            {{
                int i, prevI;
                while( true )
                {{
                    // select next note index
                    while( i == prevI && {1}.size() > 1 )
                    {{
                        Math.random2( 0, {1}.size() - 1 ) => i;
                        me.yield();
                    }}
                    i => prevI;

                    // play note
                    if( {1}[i] > 10 )
                    {{
                        Math.random2f( 0.2, 0.8 ) => {0}.strikePosition;
                        {1}[i] => Std.mtof => {0}.freq;

                        me.yield();

                        Math.random2f( 0.3, 0.4 ) + playStrong * 0.17 => {0}.strike;
                        !playStrong => playStrong;
                        jumpDelay => now;
                        
                        // signal a jump should happen
                        {5}.broadcast();
                    }}
                    else
                    {{
                        // only wait
                        jumpDelay => now; 
                    }}

                    // wait for next note
                    noteLength - jumpDelay => now;
                }}
            }}

            fun void IncreaseNoteSpeed()
            {{
                jumpDelay + 0.03::second => dur minSpeed;
                // actually let's make it longer min speed
                0.25::second => minSpeed;
                while( true )
                {{
                    // unsqueezed
                    {3} => now;

                    // --> make the jumps fast
                    0.90 *=> noteLength;
                    if( noteLength < minSpeed )
                    {{
                        minSpeed => noteLength;
                        // TODO: send a signal outward?
                        return;
                    }}
                }}
            }}
            spork ~ IncreaseNoteSpeed();

            fun void RespondToSqueezes() 
            {{
                while( true )
                {{
                    spork ~ PlayNotes() @=> Shred playNotesShred;
                    {2} => now; // when squeezed, stop jumping and playing
                    playNotesShred.exit();
                    {3} => now;
                    5::noteLength => now; // wait 3 notes after unsqueezed to resume
                }}
            }}
            spork ~ RespondToSqueezes() @=> Shred squeezeResponseShred;

            global Event scene4EndEvent;
            scene4EndEvent => now;
            squeezeResponseShred.exit();
            
            // let it die out
            while( true ) 
            {{ 
                {0}.gain() * 0.99 => {0}.gain;
                10::ms => now;
            }} 

        ", myModey, myChordNotesVar, mySqueezedEvent, myUnsqueezedEvent, jumpDelay, myJumpEvent ) );
        myChuck.SetFloatArray( myChordNotesVar, myChord );
        gameObject.AddComponent<ChuckEventListener>().ListenForEvent( myChuck, myJumpEvent, launchASeedling );

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

            // TODO: this could use some amplitude modulation
            69 => int A4;
            73 => int Cs5;
            76 => int E5;
            83 => int B5;
            90 => int Fs5;

            [A4, Cs5, E5, B5, Fs5] @=> int myNotes[];
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
                    currentGain => lpf.gain;
                    50 + 1000 * currentGain => lpf.freq;
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
            
            // turn off chord at end of movement
            global Event scene4EndEvent;
            scene4EndEvent => now;
            squeezeResponseShred.exit();
            0 => goalGain;
            
            // let it die out
            while( true ) 
            {{ 
                0 => goalGain;
                1::second => now;
            }} 
        ", mySqueezedEvent, myUnsqueezedEvent ) );
    }

    public void InformSqueezed()
    {
        myChuck.BroadcastEvent( mySqueezedEvent );
    }

    public void InformUnsqueezed()
    {
        myChuck.BroadcastEvent( myUnsqueezedEvent );
    }

    public float[] PlayArpeggio( int numNotes )
    {
        // enforce max to prevent arpeggio from going on too long 
        numNotes = (int) Mathf.Min( numNotes, 18 );
        
        // fill array with PART OF one copy, then random notes
        string[] newArpeggio = new string[numNotes];
        float[] toReturn = new float[numNotes];
        int j = 0;
        int prevJ = j;
        for( int i = 0; i < numNotes; i++ )
        {
            if( i < myArpeggio.Length / 3 ) // only take first third of array
            {
                j = i;
            }
            else
            {
                while( j == prevJ )
                {
                    j = Random.Range( 0, myArpeggio.Length );
                }
            }
            toReturn[i] = (float) myArpeggio[j];
            newArpeggio[i] = myArpeggio[j].ToString("0.0");
            prevJ = j;
        }

        string notes = "[" + string.Join( ", ", newArpeggio ) + "]";

        myChuck.RunCode( string.Format( @"
            {1} @=> float {0}[]; 

            // make non global so it's not overwriting itself
            ModalBar {2} => JCRev reverb => dac;

            0.10::second => dur noteLength;
            true => int hardPick;

            fun void PlayArray()
            {{
                for( int i; i < {0}.size(); i++ )
                {{
                    if( {0}[i] > 10 )
                    {{
                        // strike position
                        Math.random2f( 0.2, 0.8 ) => {2}.strikePosition;
                        // freq
                        {0}[i] => Std.mtof => {2}.freq;
                        // strike it!
                        Math.random2f( 0.3, 0.4 ) + 0.17 * hardPick => {2}.strike;
                        // next pick in opposite direction
                        !hardPick => hardPick;
                    }}
                    else
                    {{
                        // next pick in stronger direction
                        true => hardPick;
                    }}
        
                    noteLength => now;
                    1.09 *=> noteLength;
                }}
            }}
            PlayArray();
            5::second => now;
        ", myArpeggioNotesVar, notes, myModey ) );

        return toReturn;
    }
}
