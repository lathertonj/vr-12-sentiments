SinOsc bass => dac;
0.2 => bass.gain;

for( 38 => int i; i < 50; 1 +=> i )
{
    i => Std.mtof => bass.freq;
    1::second => now;
}