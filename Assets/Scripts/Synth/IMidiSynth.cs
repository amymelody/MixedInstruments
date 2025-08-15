using Melanchall.DryWetMidi.MusicTheory;

public struct MidiNote
{
    public bool isActive;
    public double onTime;
    public double offTime;
    public Note note;
}

public interface IMidiSynth
{
    public float Sample(MidiNote note, double noteTime);
}