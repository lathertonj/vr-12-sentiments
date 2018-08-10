//---------------|
// modal demo
// based off of mand-o-matic ( master plan ) 
// by : philipd 
// by: Ge Wang (gewang@cs.princeton.edu)
//     Perry R. Cook (prc@cs.princeton.edu)
//------------------|
// our patch

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
{
    2 => modey.preset; // I like 6 and 2
    for( int i; i < notes.size(); i++ )
    {
        if( notes[i] > 10 )
        {
            // strike position
            Math.random2f( 0.2, 0.8 ) => modey.strikePosition;
            // freq
            notes[i] => Std.mtof => modey.freq;
            // strike it!
            Math.random2f( 0.3, 0.4 ) + 0.17 * hardPick => modey.strike;
            // next pick in opposite direction
            !hardPick => hardPick;
        }
        else
        {
            // next pick in stronger direction
            true => hardPick;
        }
        
        noteLength => now;
        
        // turn off? this isn't it
        1 => modey.damp;
    }
    

}


// our main loop

while( true )
{
    <<< "base", "" >>>;
    // play a base
    PlayArray( bases[ Math.random2( 0, bases.size() - 1 ) ] );
    
    // with small chance, don't play a top (== play a base twice in a row)
    if( Math.randomf() < 0.2 )
    {
        continue;
    }
    
    // with a medium chance, rest once
    if( Math.randomf() < 0.45 )
    {
        <<< "rest","" >>>;
        noteLength => now;
    }
    
    <<< "top","" >>>;
    // play a top
    PlayArray( tops[ Math.random2( 0, tops.size() - 1 ) ] );
    
    // with a small chance, play a super top
    // PARAMETER TO MODULATE: freq of super top
    if( Math.randomf() < 0.2 ) 
    {
        <<< "super top", "" >>>;
        // PARAMETER TO MODULATE: upper limit of allowed super top 
        // (the array should only get more and more intense left to right)
        PlayArray( supertops[ Math.random2( 0, supertops.size() - 1 ) ] );
    }
    
    // with a small chance, rest a while
    if( Math.randomf() < 0.05 )
    {
        <<< "long rest", "" >>>;
        Math.random2( 1, 3 )::noteLength => now;
    }
        
}