using Melanchall.DryWetMidi.Common;
using System.Collections.Generic;
using UnityEngine;

public class MidiSampleAmp : MidiAmp
{
    Dictionary<SevenBitNumber, AudioClip> m_Samples = new Dictionary<SevenBitNumber, AudioClip>();

    public void AssignSample(SevenBitNumber noteNumber, AudioClip sample)
    {
        m_Samples[noteNumber] = sample;
    }

    public override void NoteOn(SevenBitNumber noteNumber)
    {
        base.NoteOn(noteNumber);

        if (m_Samples.TryGetValue(noteNumber, out var sample))
        {
            audioSource.PlayOneShot(sample);
        }
    }
}
