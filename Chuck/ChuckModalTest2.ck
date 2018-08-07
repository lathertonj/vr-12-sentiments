//---------------|
// modal demo
// based off of mand-o-matic ( master plan ) 
// by : philipd 
// by: Ge Wang (gewang@cs.princeton.edu)
//     Perry R. Cook (prc@cs.princeton.edu)
//------------------|
// our patch

ModalBar modey => JCRev r => Echo a => Echo b => Echo c => dac;

// set the gain
.5 => r.gain;
// set the reverb mix
.05 => r.mix;
// set max delay for echo
1000::ms => a.max => b.max => c.max;
// set delay for echo
750::ms => a.delay => b.delay => c.delay;
// set the initial effect mix
0.0 => a.mix => b.mix => c.mix;

<<< "preset:", modey.preset() >>>;

// shred to modulate the mix
fun void echo_Shred( )
{ 
    0.0 => float decider => float mix => float old => float inc;

    // time loop
    while( true )
    {
        Math.random2f( 0, 1 ) => decider;
        if( decider < .35 ) 0.0 => mix;
        else if( decider < .55 ) .08 => mix;
        else if( decider < .8 ) .5 => mix;
        else .15 => mix;

        // find the increment
        (mix-old)/1000.0 => inc; 1000 => int n;
        // time loop
        while( n-- )
        {
            // set the mix for a, b, c
            Math.max( old + inc, 0 ) => old => a.mix => b.mix => c.mix;
            1::ms => now;
        }
        // remember the old
        mix => old;
        // let time pass until the next iteration
        Math.random2(2,6)::second => now;
    }
}

// let echo shred go
spork ~ echo_Shred();

// scale
[ 0, 2, 4, 7, 9, 11 ] @=> int scale[];

// our main loop
while( true )
{
    // presets
    5 => modey.preset;
    
    // position
    Math.random2f( 0.2, 0.8 ) => modey.strikePosition;
    // frequency... random note from octave
    scale[Math.random2(0,scale.cap()-1)] => int note;
    // plus random octave + bass note 45
    Std.mtof( 45 + Math.random2(0,4)*12 + note ) => modey.freq;

    // pluck it!
    Math.random2f( 0.2, 0.6 ) => modey.strike;

    // wait a random amount of time: 0.5s, 0.25s, 0.125s, or....
    if( Math.randomf() > 0.8 )
    { 500::ms => now; }
    else if( Math.randomf() > .925 )
    { 250::ms => now; }
    else if( Math.randomf() > .05 )
    { .125::second => now; }
    else
    {
        // with a small chance, do a trill like thing
        
        1 => int i => int pick_dir;
        // how many times: 4, 8, 12, 16, or 20
        4 * Math.random2( 1, 5 ) => int pick;
        0.0 => float pluck;
        0.65 / pick => float inc;
        // time loop. each pluck gets successfuly louderish,
        // plus some randomness
        // plus louder in one direction than the other
        for( ; i < pick; i++ )
        {
            75::ms => now;
            Math.random2f(.2,.3) + i*inc => pluck;
            pluck => modey.stickHardness;
            pluck + -.2 * pick_dir => modey.strike;
            // simulate pluck direction
            !pick_dir => pick_dir;
        }
        // let time pass for final pluck
        75::ms => now;
    }
}