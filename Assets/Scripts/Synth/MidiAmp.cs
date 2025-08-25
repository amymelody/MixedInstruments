using Melanchall.DryWetMidi.Common;
using UnityEngine;

public class MidiAmp : Amp
{
    protected MidiNote[] m_Notes;

    protected override void Awake()
    {
        base.Awake();

        m_Notes = new MidiNote[SevenBitNumber.MaxValue];
        for (var i = 0; i < m_Notes.Length; i++)
        {
            m_Notes[i].noteNumber = (SevenBitNumber)i;
        }
    }

    public virtual void NoteOn(SevenBitNumber noteNumber)
    {
        m_Notes[noteNumber].isActive = true;
        m_Notes[noteNumber].onTime = AudioSettings.dspTime;
    }

    public virtual void NoteOff(SevenBitNumber noteNumber)
    {
        m_Notes[noteNumber].isActive = false;
        m_Notes[noteNumber].offTime = AudioSettings.dspTime;
    }
}
