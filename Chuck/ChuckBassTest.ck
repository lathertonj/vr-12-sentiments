SinOsc bass => ADSR a => dac;
a.set( 10::ms, 30::ms, 0.95, 30::ms );
0.25 => a.gain;

float currentBassFreq;
fun void SetBassFreq()
{
    SinOsc lfo => blackhole;
    0.932545 => lfo.freq;
    
    while( true )
    {
        currentBassFreq + 0.73 * lfo.last() => bass.freq;
        1::samp => now;
    }
}
spork ~ SetBassFreq();

42 => int lowNote;
lowNote - 7 => int lowerNote;



while( true ) {

    lowNote => Math.mtof => currentBassFreq;

    a.keyOn(1);
    8::second => now;
    a.keyOff(1);

    a.releaseTime() => now;


    lowerNote => Math.mtof => currentBassFreq;
    a.keyOn(1);

    8::second => now;
    a.keyOff(1);
    a.releaseTime() => now;

}

