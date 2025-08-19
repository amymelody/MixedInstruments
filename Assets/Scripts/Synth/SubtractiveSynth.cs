public class SubtractiveSynth : IMidiSynth
{
    Oscillator m_Osc1;

    public SubtractiveSynth()
    {
        m_Osc1 = new Oscillator();
    }

    public float Sample(MidiNote note, double noteTime)
    {
        var frequency = NoteUtils.FundementalFrequencies[note.note.NoteNumber];
        var phase = ((float)(noteTime % (1.0d / (double)frequency))) * frequency;
        return m_Osc1.Sample(phase);
    }
}