﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene6Seedling : MonoBehaviour
{
    private string myTryToPlayEvent, myDidPlayEvent;
    private ChuckSubInstance myChuck;
    private ParticleSystem mySharedParticleSystem;

    // Use this for initialization
    void Start()
    {
        mySharedParticleSystem = GetComponentInParent<ParticleSystem>();
        // only start sound after seedlings move around a bit
        Invoke( "InitChuck", 3 );
    }
    void InitChuck()
    {
        myChuck = GetComponent<ChuckSubInstance>();
        myTryToPlayEvent = myChuck.GetUniqueVariableName();
        myDidPlayEvent = myChuck.GetUniqueVariableName();
        myChuck.RunCode( string.Format( @"

            66 => int Fs;
            68 => int Gs;
            73 => int Cs;
            [Fs, Gs, Cs] @=> int notes[];
            100::ms => dur minWaitTime;

            fun void PlayANote( int note )
            {{
                ModalBar modey => JCRev reverb => dac;
                0.28 => reverb.mix;
                Math.random2f( 0.2, 0.8 ) => modey.strikePosition;
                note => Std.mtof => modey.freq;

                me.yield();

                Math.random2f( 0.3, 0.5 ) => modey.strike;

                3::second => now;
            }}

            global Event {0}, {1};
            global int scene6NoteIndex;
            global float timeOfLastNoteSeconds;

            fun void RespondToEvents()
            {{
                while( true )
                {{
                    {0} => now;
                    if( ( now / 1::second ) > timeOfLastNoteSeconds + ( minWaitTime * Math.random2f( 1, 1.2 ) / 1::second ) )
                    {{
                        spork ~ PlayANote( notes[scene6NoteIndex] );
                        scene6NoteIndex++; if( scene6NoteIndex >= notes.size() ) {{ 0 => scene6NoteIndex; }}
                        now / 1::second => timeOfLastNoteSeconds;
                        {1}.broadcast();
                    }}
                }}
            }}
            spork ~ RespondToEvents() @=> Shred eventResponder;
            global Event scene6StopMakingSound;
            scene6StopMakingSound => now;
            eventResponder.exit();
            // reverb tail
            10::second => now;
        ", myTryToPlayEvent, myDidPlayEvent ) );

        ChuckEventListener animateSuccessfulNotes = gameObject.AddComponent<ChuckEventListener>();
        animateSuccessfulNotes.ListenForEvent( myChuck, myDidPlayEvent, AnimateHavingPlayedEvent );
    }

    private Vector3 lastContactPoint = Vector3.zero;
    void OnCollisionEnter( Collision collision )
    {
        if( myChuck != null )
        {
            myChuck.BroadcastEvent( myTryToPlayEvent );
        }
        lastContactPoint = collision.contacts[0].point;
    }

    void AnimateHavingPlayedEvent()
    {
        // show a flare if I sounded my note
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.position = mySharedParticleSystem.transform.InverseTransformPoint( lastContactPoint );
        emitParams.velocity = 0.15f * Vector3.up;
        mySharedParticleSystem.Emit( emitParams, count: 1 );
    }
}
