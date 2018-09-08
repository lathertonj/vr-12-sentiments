class AbsDistortion extends Chugen
{
    fun float tick( float in )
    {
        //return in;
        return 1.9 * in / ( 1 + Math.fabs( in ) );
    }
}

class NoDistortion extends Chugen
{
    fun float tick( float in )
    {
        return in;
    }
}

class Abs3Distortion extends Chugen
{
    fun float tick( float in )
    {
        in * in * in => float three;
        //return in;
        return three * 1.5 / ( 1 + Math.fabs( three ) );
    }
}

class TanDistortion extends Chugen
{
    fun float tick( float in )
    {
        in * in => float square;
        in * square => float cubic;
        cubic * square => float five;
        five * square => float seven;
        return 0.5 * (in - cubic / 3 + 2 * five / 15 - 17 * seven / 315);
    }
}

class SailLead extends Chubgraph
{
    SawOsc osc1 => Gain common => LPF lpf => TanDistortion distortion => ADSR a => Gain out => outlet;
    SawOsc osc2 => common;
    SqrOsc osc3 => common;
    0.35 => osc1.gain;
    0.45 => osc2.gain;
    0.20 => osc3.gain;
    1.2 => common.gain; // input to overdrive to distort...
    //1 => overdrive.sync; // SinOsc overdrive
    // dyno.compress();  // Dyno dyno
    13 => lpf.Q;
    0.5 => out.gain;
    staccato( true );    
    
    200 => float goalFreq;
    200 => float currentFreq;
    0.15 => float slewFreq;
    false => int noteIsOn;
    
    fun void staccato( int doStaccato )
    {
        if( doStaccato )
        {
            a.set( 20::ms, 150::ms, 0.3, 1::ms );
        }
        else
        {
            a.set( 20::ms, 5::ms, 0.9, 20::ms );
        }
    }
    
    fun void SetFreq()
    {
        SinOsc pitchLfo => blackhole;
        1.1 => pitchLfo.freq;
        while( true )
        {
            slewFreq * ( goalFreq - currentFreq ) +=> currentFreq;
            // 0.5% of current frequency
            currentFreq + 0.005 * currentFreq * pitchLfo.last() => 
                float waveringFreq;
            waveringFreq => osc1.freq;
            waveringFreq / 2 => osc2.freq;
            waveringFreq => osc3.freq;
            
            // what is "80% of the way up"?
            Math.min( waveringFreq * 32, 20000 ) => lpf.freq;
            1::ms => now;
        }
    }
    spork ~ SetFreq();
    
    fun float freq( float f )
    {
        f => goalFreq;
        if( noteIsOn )
        {
            0.026 => slewFreq;
        }
        else
        {
            0.15 => slewFreq;
        }

        // putting this only here instead of in the while loop
        // makes the attack sound harder because
        // the resonant filter doesn't glide
        //Math.min( goalFreq * 32, 20000 ) => lpf.freq;
        
        return f;
    }
    
    fun float freq()
    {
        return goalFreq;
    }
    
    fun void noteOn()
    {
        if( !noteIsOn )
        {
            1 => a.keyOn;
            goalFreq * 8 => currentFreq; // higher = harder attack
            true => noteIsOn;
        }
        
    }
    
    fun void noteOff()
    {
        1 => a.keyOff;
        false => noteIsOn;
    }
}

SailLead lead => JCRev reverb => dac;
SailLead bass => reverb;
0.02 => reverb.mix;
220 => lead.freq;

[ 
  0,  0,  0,
  0,  0,  0,
 64, 64, 64, 
 67, 67, 67, 
 69,  0,  0, 
 71,  0,  0, 
 69,  0, 67,
 64,  0,  0
] @=> int notes[];

0.16::second => dur noteLength;
0.04::second => dur endLength;

fun void PlayBass()
{
    false => bass.staccato;
    while( true )
    {
        40 => Std.mtof => bass.freq;
        bass.noteOn();
        6::noteLength - endLength => now;
        bass.noteOff();
        endLength => now;
        17::noteLength => now;
        52 => Std.mtof => bass.freq;
        bass.noteOn();
        1::noteLength => now;
    }
}
spork ~ PlayBass();

while( true )
{
    for( int i; i < notes.size(); i++ )
    {
        if( notes[i] > 10 )
        {
            notes[i] => Std.mtof => lead.freq;
            lead.noteOn();
        }
        
        noteLength - endLength => now;
        
        if( notes[i] > 10 )
        {
            lead.noteOff();
        }
        endLength => now;
    }
    
}