using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BassPlayer : MonoBehaviour
{
    private ChuckSubInstance myChuck;

    // Use this for initialization
    void Start()
    {
        myChuck = GetComponent<ChuckSubInstance>();
        myChuck.RunCode( @"
            SinOsc bass => NRev reverb => dac;
            0.05 => reverb.mix; 
            0.05 => float maxBassGain;
            //[47.0, 42.0, 38.0, 40.0 ] @=> float bassNotes[];

// for some reason, the bass is a lot louder on note 47 than note 42. I can't figure out why. Maybe it's these speakers
            47 => global float bassNote;
            0 => global float bassGain;

            47 => float currentBassNote;

            fun void SlewBass() 
            {
                0.15 => float bassSlew;

                while( true )
                {
                    currentBassNote + bassSlew * ( bassNote - currentBassNote ) => currentBassNote;
                    currentBassNote => Std.mtof => bass.freq;
                    maxBassGain * bassGain => bass.gain;
                    1::ms => now;
                }
            }
            spork ~ SlewBass();

            while( true ) { 1::second => now; }
        " );
    }

    // Update is called once per frame
    void Update()
    {

    }
}
