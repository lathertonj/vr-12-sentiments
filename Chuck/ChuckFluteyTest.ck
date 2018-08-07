// STK Clarinet
// (also see examples/event/polyfony2.ck)

// patch
Clarinet clair => LPF lpf => JCRev r => dac;
.25 => r.gain;
.08 => r.mix;



42 + 3 * 12 => int baseNote;

baseNote + 2 * 12 => Math.mtof => lpf.freq;


// our notes
[ 0, 0, 0, 0, 0, 0, 0, 0, 0, baseNote + 0, baseNote + 7, baseNote + 5 ] @=> int notes[];

// infinite time-loop
while( true )
{
    
    for( int i; i < notes.cap(); i++ )
    {
        if( notes[i] != 0 )
        {
            spork ~ play( notes[i], Math.random2f( .6, .9 ), 120::ms );
        }
        150::ms => now;
    }
}

// basic play function (add more arguments as needed)
fun void play( float note, float velocity, dur onTime )
{
    // clear
    clair.clear( 1.0 );
    
    // set
    Math.random2f( 0.15, 0.25 ) => clair.reed;
    Math.random2f( 0.7, 0.85 ) => clair.noiseGain;
    Math.random2f( 0, 2 ) => clair.vibratoFreq;
    Math.random2f( 0, 0.5 ) => clair.vibratoGain;
    Math.random2f( 0.5, 0.8 ) => clair.pressure;
    
    // print
    <<< "---", "" >>>;
    <<< "reed stiffness:", clair.reed() >>>;
    <<< "noise gain:", clair.noiseGain() >>>;
    <<< "vibrato freq:", clair.vibratoFreq() >>>;
    <<< "vibrato gain:", clair.vibratoGain() >>>;
    <<< "breath pressure:", clair.pressure() >>>;
    
    // start the note
    Std.mtof( note ) => clair.freq;
    velocity => clair.noteOn;
    
    onTime => now;
    velocity => clair.noteOff;
}