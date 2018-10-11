using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene6SeedlingSonifier : MonoBehaviour
{
    ChuckSubInstance myChuck;
    bool hasChuckInit = false;

    // Use this for initialization
    void Start()
    {
        myChuck = GetComponent<ChuckSubInstance>();
        // only start sound after seedlings move around a bit
        Invoke( "InitChuck", 2 );
    }

    void InitChuck()
    {
        // do things to chuck
        myChuck.RunCode( @"
			ModalBar modey => JCRev reverb => dac;
            global Event scene6PlayASeedling;
			66 => int Fs;
			68 => int Gs;
			73 => int Cs;
			[Fs, Gs, Cs] @=> int notes[];
			100::ms => dur minWaitTime;

            true => int playStrong;

			// TODO: decide whether the note should be random instead. it probably should be...
            fun void PlayNotes()
            {
                while( true )
                {
                    for( int i; i < notes.size(); i++ )
                    {
						scene6PlayASeedling => now;

                        // play note
                        if( notes[i] > 10 )
                        {
                            Math.random2f( 0.2, 0.8 ) => modey.strikePosition;
                            notes[i] => Std.mtof => modey.freq;

                            me.yield();

                            Math.random2f( 0.3, 0.4 ) + playStrong * 0.17 => modey.strike;
                            !playStrong => playStrong;
                        }


                        // wait for next note
                        minWaitTime * Math.random2f( 1, 1.2 ) => now;
                    }
                }
            }
			// spork ~ PlayNotes();

			int i;
			fun void PlayANote()
			{
				ModalBar modey => reverb;
				Math.random2f( 0.2, 0.8 ) => modey.strikePosition;
				notes[i] => Std.mtof => modey.freq;

				me.yield();

				Math.random2f( 0.3, 0.4 ) + playStrong * 0.17 => modey.strike;
				!playStrong => playStrong;

				i++; if( i >= notes.size() ) { 0 => i; }
				3::second => now;
			}

            
            while( true ) 
			{ 
				scene6PlayASeedling => now;
				spork ~ PlayANote();
				minWaitTime => now;
			}
            
		" );

        hasChuckInit = true;
    }

    public void PlayASeedling()
    {
        if( hasChuckInit )
        {
            // send a broadcast to chuck
            myChuck.BroadcastEvent( "scene6PlayASeedling" );
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
