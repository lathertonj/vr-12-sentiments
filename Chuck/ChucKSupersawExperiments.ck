// using a carrier wave "saw oscillator for example," 
// and modulating its signal using a comb filter 
// where the filter cutoff frequency is usually 
// modulated with an LFO, which the LFO's depth (or amplitude) 
// is equal to the saw oscillator's current frequency. 


// It can also be done by using a copied signal and 
// have the copy run throught a delay which the 
// delay's time is modulated again by an LFO where the 
// LFO's depth is equal to the saw oscillator's current frequency.

class Supersaw extends Chubgraph
{
    SawOsc osc => Gain out => outlet;
    5 => int numDelays;
    1.0 / (1 + numDelays) => out.gain;
    DelayA theDelays[numDelays];
    SinOsc lfos[numDelays];
    dur baseDelays[numDelays];
    float baseFreqs[numDelays];
    for( int i; i < numDelays; i++ )
    {
        osc => theDelays[i] => out;
        0.15::second => theDelays[i].max;
        // crucial to modify!
        Math.random2f( 0.001, 0.002 )::second => baseDelays[i];
        lfos[i] => blackhole;
//        Math.random2f( -0.1, 0.1) => baseFreqs[i];
        Math.random2f( 0, pi ) => lfos[i].phase;
        
    }
    0.05::second => dur baseDelay;
    0.333 => float baseFreq;
    1 => float lfoGain;
    
    fun void AttachLFOs()
    {
        while( true )
        {
            for( int i; i < numDelays; i++ )
            {
                baseFreq + baseFreqs[i] => lfos[i].freq;
                lfoGain * lfos[i].last()::second + baseDelay + baseDelays[i] 
                    => theDelays[i].delay;
            }
            1::ms => now;
        }
    }
    spork ~ AttachLFOs();
    
    
    SinOsc pitchLFO => blackhole;
    0.77 => pitchLFO.freq;
    1.0 / 300 => float pitchLFODepth;
    440 => float basePitch;
    
    fun void FreqMod()
    {
        while( true )
        {
            // calc freq
            basePitch + ( basePitch * pitchLFODepth ) 
                * pitchLFO.last() => float f;
            // set
            f => osc.freq;
            1.0 / f => lfoGain; // seconds per cycle == gain amount
            // wait
            1::ms => now;
        }
    }
    spork ~ FreqMod();
    
    fun void freq( float f )
    {
        f => basePitch;
    }
    
    fun void delay( dur d )
    {
        d => baseDelay;
    }
    
    fun void timbreLFO( float f )
    {
        f => baseFreq;
    }
    
    fun void pitchLFORate( float f )
    {
        f => pitchLFO.freq;
    }
    
    fun void pitchLFODepthMultiplier( float r )
    {
        r => pitchLFODepth;
    }
}

HPF hpf => LPF lpf => Gain ampMod => NRev reverb => dac;
1800 => hpf.freq;
5300 => lpf.freq;
200 => hpf.freq;
6000 => lpf.freq;
0.05 => reverb.mix;

fun void AmpMod()
{
    SinOsc lfo => blackhole;
    0.13 => lfo.freq;
    while( true )
    {
         0.85 + 0.15 * lfo.last() => ampMod.gain;
         1::ms => now;
    }
}
spork ~ AmpMod();

// TODO: this could use some amplitude modulation
Supersaw mySaws[4];
[63, 67, 70, 74] @=> int notes[];
//[62, 65, 69, 72] @=> notes;
//[60, 63, 67, 70] @=> notes;

69 => int A4;
71 => int B4;
74 => int D5;
76 => int E5;
78 => int Fs5;
79 => int G5;
[A4, D5, E5, G5] @=> notes;


for( int i; i < 4; i++ )
{
    notes[i] + -24 => Std.mtof => mySaws[i].freq;
    // how much freq waves up and down as a % of current freq
    1.2 / 300 => mySaws[i].pitchLFODepthMultiplier; 
    // how quickly the freq lfo moves
    3.67 => mySaws[i].pitchLFORate;
    // higher == wider pitch spread
    0.33 => mySaws[i].timbreLFO; 
    // basic loudness
    0.035 => mySaws[i].gain;
    
    mySaws[i] => hpf;
}

0.12::second * 2 => dur noteLength;

Supersaw mySaw => ADSR myAdsr => hpf;
myAdsr.set( 0.1::noteLength, 0.3::noteLength, 0.85, 0.05::noteLength );


// how much freq waves up and down as a % of current freq
1.2 / 300 => mySaw.pitchLFODepthMultiplier; 
// how quickly the freq lfo moves
3.67 => mySaw.pitchLFORate;
// higher == wider pitch spread
0.33 => mySaw.timbreLFO; 
// basic loudness
0.05 => mySaw.gain;


[63.0, 67, 70, 74] @=> float chordNotes[];
[63.0, 67, 70, 74] @=> float currentChordNotes[];
[0.05, 0.07, 0.09, 0.11] @=> float chordSlews[];
fun void SlewChordFreqs()
{
    while( true )
    {
        for( int i; i < 4; i++ )
        {
            currentChordNotes[i] + chordSlews[i] * 
                ( chordNotes[i] - currentChordNotes[i] ) => currentChordNotes[i];
            currentChordNotes[i] - 12 => Std.mtof => mySaws[i].freq;
        }
        1::ms => now;
    }
}
spork ~ SlewChordFreqs();



[G5, Fs5, Fs5, G5] @=> int melodyNotes[];

while( true )
{
    for( int i; i < melodyNotes.size(); i++ )
    {
        melodyNotes[i] => Std.mtof => mySaw.freq;
        //myAdsr.keyOn( true );
        noteLength - myAdsr.releaseTime() => now;
        //myAdsr.keyOff( true );
        myAdsr.releaseTime() => now;
    }
}