using UnityEngine;

public class SubtractiveSynth : IMidiSynth
{
    public float Sample(MidiNote note, double noteTime)
    {
        var frequency = NoteUtils.FundementalFrequencies[note.note.NoteNumber];
        var phase = ((float)(noteTime % (1.0d / (double)frequency))) * frequency;
        return Mathf.Sin(phase * MathUtils.TwoPi);
    }
}