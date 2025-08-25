using Melanchall.DryWetMidi.Common;

public struct MidiNote
{
    public bool isActive;
    public double onTime;
    public double offTime;
    public SevenBitNumber noteNumber;
}

public interface IMidiSynth
{
    public float Sample(MidiNote note, double noteTime);
}