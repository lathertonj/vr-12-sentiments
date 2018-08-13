//---------------|
// modal demo
// based off of mand-o-matic ( master plan ) 
// by : philipd 
// by: Ge Wang (gewang@cs.princeton.edu)
//     Perry R. Cook (prc@cs.princeton.edu)
//------------------|
// our patch

// Use in scene: how much you are leaned over 
// (hands + head) from the initial starting point
// determines the next base pitch (plus
// maybe some base oscillation)

// OR. oscillate a bit but also generally
// trend upward in pitch the more sunlight you catch!

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

15.3 => float offset;
[B5 + offset, D6 + offset, B5 + offset] @=> float notes[];

fun void PlayArrayInterp( float notes[], int numInterpNotes )
{
    5 => modey.preset; // I like 3 and 5
    for( int i; i < notes.size() - 1; i++ )
    {
        if( notes[i] > 10 )
        {
            ( notes[i+1] - notes[i] ) * 1.0 / numInterpNotes => float interpAmount;
            for( int j; j < numInterpNotes; j++ )
            {

                // strike position
                Math.random2f( 0.2, 0.8 ) => modey.strikePosition;
                // freq
                notes[i] + j * interpAmount => Std.mtof => modey.freq;
                // strike it!
                Math.random2f( 0.3, 0.4 ) + 0.17 * hardPick => modey.strike;
                // next pick in opposite direction
                !hardPick => hardPick;
                // wait
                noteLength => now;
            }
        }
        else
        {
            // next pick in stronger direction
            true => hardPick;
            numInterpNotes::noteLength => now;
        }
        
        
        
        // turn off? this isn't it
        1 => modey.damp;
    }
    

}


// our main loop

while( true )
{
    PlayArrayInterp( notes, 36 );
}