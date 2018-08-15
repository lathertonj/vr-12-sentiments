using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene2Advancer : MonoBehaviour
{
    public float chordSwitchCutoff = 15f;
    public float[] otherCutoffs;
    public SonifySunbeamInteractors[] hands;
    public Light sun;
    public float sunInitialValue, sunPrecutoffValue, sunPostCutoffValue;
    private float sunH, sunS, sunV;
    public Transform sunbeamPrefab;
    public Transform sunbeamHolder;

    private ChuckSubInstance myChuck;

    // Use this for initialization
    void Start()
    {
        myChuck = GetComponent<ChuckSubInstance>();
        StartChuck();

        // every so often but not necessary every frame
        // (every 50ms)
        InvokeRepeating( "UpdateSunbeam", 0.0f, 0.05f );

        Color.RGBToHSV( sun.color, out sunH, out sunS, out sunV ); 
    }

    // Update is called once per frame
    private bool haveSwitchedChords = false;
    void Update()
    {
        if( !haveSwitchedChords )
        {
            sun.color = Color.HSVToRGB( sunH, sunS, 
                SunbeamInteractors.sunbeamAccumulated.Map( 
                    0,               chordSwitchCutoff, 
                    sunInitialValue, sunPrecutoffValue 
                ) 
            );
        }

        if( !haveSwitchedChords && SunbeamInteractors.sunbeamAccumulated > chordSwitchCutoff )
        {
            SwitchToSecondHalf();
            haveSwitchedChords = true;

            sun.color = Color.HSVToRGB( sunH, sunS, sunPostCutoffValue );
        }
    }

    void SwitchToSecondHalf()
    {
        // spawn many more light columns
        for( int i = 0; i < 10; i++ )
        {
            Quaternion newRotation = Quaternion.AngleAxis( Random.Range( -25, 25 ), Vector3.left ) * 
                                     Quaternion.AngleAxis( Random.Range( -25, 25 ), Vector3.forward );
            Vector3 newPosition = new Vector3( 
                Random.Range( -1.5f, 1.5f ), 
                0, 
                Random.Range( -1.5f, 1.5f ) 
            ) + sunbeamHolder.position;

            Instantiate( sunbeamPrefab, newPosition, newRotation, sunbeamHolder );
        }
        
        // prevent them from fading
        SunbeamController.shouldFade = false;
        
        // make it quicker to interact with one (from 5s to 0.1s)
        SunbeamInteractors.sunbeamFadeinTime = 0.1f;

        // TODO: modify visuals more?

        foreach( SonifySunbeamInteractors hand in hands )
        {
            hand.AdvanceToSecondHalf();
        }
    }

    void UpdateSunbeam()
    {
        myChuck.SetFloat( "sunbeamAccumulation", SunbeamInteractors.sunbeamAccumulated );
    }

    void StartChuck()
    {
        myChuck.RunCode( string.Format( @"
            ModalBar modey => JCRev r => dac;

            // set the gain
            .9 => r.gain;
            // set the reverb mix
            .05 => r.mix;

            0.12::second => dur noteLength;
            true => int hardPick;

            69 => int A4;
            71 => int B4;
            74 => int D5;
            76 => int E5;
            78 => int Fs5;
            79 => int G5;
            81 => int A5;
            83 => int B5;
            86 => int D6;

            [[A4, B4], [A4, B4, D5], [0, B4, D5]] @=> int bases[][];
            [[E5, G5], [Fs5], [E5], [Fs5, G5], [G5]] @=> int tops[][];
            [[Fs5, G5, A5], [G5, A5, B5], [A5, B5, D6]] @=> int supertops[][];

            fun void PlayArray( int notes[] )
            {{
                2 => modey.preset; // I like 6 and 2
                for( int i; i < notes.size(); i++ )
                {{
                    if( notes[i] > 10 )
                    {{
                        // strike position
                        Math.random2f( 0.2, 0.8 ) => modey.strikePosition;
                        // freq
                        notes[i] => Std.mtof => modey.freq;
                        // strike it!
                        Math.random2f( 0.3, 0.4 ) + 0.17 * hardPick => modey.strike;
                        // next pick in opposite direction
                        !hardPick => hardPick;
                    }}
                    else
                    {{
                        // next pick in stronger direction
                        true => hardPick;
                    }}
        
                    noteLength => now;
        
                    // turn off? this isn't it
                    1 => modey.damp;
                }}
    

            }}


            // our main loop
            global float sunbeamAccumulation;

            while( true )
            {{
                // play a base
                PlayArray( bases[ Math.random2( 0, bases.size() - 1 ) ] );
    
                // with small chance, don't play a top (== play a base twice in a row)
                if( Math.randomf() < 0.2 )
                {{
                    continue;
                }}

                // below the first cutoff, only play bottoms
                if( sunbeamAccumulation < {0} )
                {{
                    continue;
                }}
    
                // with a medium chance, rest once
                if( Math.randomf() < 0.45 )
                {{
                    noteLength => now;
                }}
    
                // play a top
                PlayArray( tops[ Math.random2( 0, tops.size() - 1 ) ] );
    
                // with a small chance, play a super top
                // PARAMETER TO MODULATE: freq of super top
                0.0 => float superTopChance;
                0 => int maxSuperTopAllowed;
                if( sunbeamAccumulation > {3} )
                {{
                    0.6 => superTopChance;
                    supertops.size() - 1 => maxSuperTopAllowed;
                }}
                else if( sunbeamAccumulation > {2} )
                {{
                    0.4 => superTopChance;
                    1 => maxSuperTopAllowed;
                }}
                else if( sunbeamAccumulation > {1} )
                {{
                    0.2 => superTopChance;
                    0 => maxSuperTopAllowed;
                }}
                if( Math.randomf() < superTopChance ) 
                {{
                    // PARAMETER TO MODULATE: upper limit of allowed super top 
                    // (the array should only get more and more intense left to right)
                    PlayArray( supertops[ Math.random2( 0, maxSuperTopAllowed ) ] );
                }}
    
                // with a small chance, rest a while
                if( Math.randomf() < 0.05 )
                {{
                    Math.random2( 1, 3 )::noteLength => now;
                }}
        
            }}

        ", otherCutoffs[0], otherCutoffs[1], otherCutoffs[2], otherCutoffs[3] ) );
    }
}
