Shakers s => JCRev reverb => dac;
0.05 => reverb.mix;

// params
0.7 => s.decay; // 0 to 1
50 => s.objects; // 0 to 128
11 => s.preset; // 0 to 22.  0 is good for galloping. 
// 11 is good for eighths

0.12::second => dur tatum;

fun void OnBeats()
{
    while( true )
    {
        Math.random2f( 0.7, 0.9 ) => s.energy;
        1 => s.noteOn;
        2::tatum => now;
    
        Math.random2f( 0.3, 0.5 ) => s.energy;
        1 => s.noteOn;
        2::tatum => now;
    }

}
spork ~ OnBeats();

fun void OffBeats()
{
    1::tatum => now;
    
    while( true )
    {
        Math.random2f( 0.1, 0.3 ) => s.energy;
        1 => s.noteOn;
        2::tatum => now;
    }
}

64 => int numInFirstPart;
for( int i; i < numInFirstPart; i++ )
{
    0.05 + 0.2 * i / numInFirstPart => s.gain;
    1::tatum => now;
}
spork ~ OffBeats();

32 => int numInSecondPart;
for( int i; i < numInSecondPart; i++ )
{
    0.25 + 0.75 * Math.pow( i * 1.0 / numInSecondPart, 2 ) => s.gain;
    1::tatum => now;
}
