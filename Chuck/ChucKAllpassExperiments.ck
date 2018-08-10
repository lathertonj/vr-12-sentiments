class Allpass extends Chubgraph
{
    Gain z => Delay zDelayed1;
    1::samp => zDelayed1.delay;
    Gain minusK, posK;
    0.1 => posK.gain;
    -0.1 => minusK.gain;
    inlet => z; // z = x + ... 
    zDelayed1 => minusK => z; //... -k z[n-1]
    z => posK => outlet; // y = kz + ...
    zDelayed1 => outlet; // z[n-1]
    
    fun void K( float newK )
    {
        Std.clampf( newK, 0.001, 0.998 ) => newK;
        newK => posK.gain;
        -newK => minusK.gain;
    }
}

Allpass myAllPass;
0.4 => myAllPass.K;

SawOsc myOsc => dac;
0.1 => myOsc.gain;
1::second => now;
//myOsc =< dac;
SawOsc otherOsc => myAllPass => dac;
0.1 => otherOsc.gain;
1::second => now;