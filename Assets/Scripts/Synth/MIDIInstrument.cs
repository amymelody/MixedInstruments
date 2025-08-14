using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MIDIInstrument : MonoBehaviour
{
    struct ActiveNoteInfo
    {
        public bool isActive;
        public double onTime;
        public double offTime;
        public float fundamentalFrequency;
    }

    int m_SampleRate;

    ActiveNoteInfo[] m_ActiveNotes;

    void Awake()
    {
        m_SampleRate = AudioSettings.outputSampleRate;

        m_ActiveNotes = new ActiveNoteInfo[SevenBitNumber.MaxValue];
        var freqCount = NoteUtils.FundementalFrequencies.Length;
        for (var i = 0; i < freqCount; i++)
        {
            m_ActiveNotes[i].fundamentalFrequency = NoteUtils.FundementalFrequencies[i];
        }
    }

    public void NoteOn(Note note)
    {
        m_ActiveNotes[note.NoteNumber].isActive = true;
        m_ActiveNotes[note.NoteNumber].onTime = AudioSettings.dspTime;
    }

    public void NoteOff(Note note)
    {
        m_ActiveNotes[note.NoteNumber].isActive = false;
        m_ActiveNotes[note.NoteNumber].offTime = AudioSettings.dspTime;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (channels != 2) // currently only support for 2 channels
            return;

        for (var i = 0; i < m_ActiveNotes.Length; i++)
        {
            var activeNote = m_ActiveNotes[i];
            if (!activeNote.isActive)
                continue;

            var frequency = activeNote.fundamentalFrequency;
            var noteTime = AudioSettings.dspTime - activeNote.onTime;
            var currentPhaseStart = (float)(noteTime % (1.0d / (double)frequency));
            var currentDataStep = 0;
            for (var j = 0; j < data.Length; j = j + 2)
            {
                var phase = (currentPhaseStart + (float)currentDataStep / m_SampleRate) * frequency;
                var sample = Sample(phase);
                data[j] += sample;
                data[j + 1] = sample;
                currentDataStep++;
            }
        }
    }

    float Sample(float phase)
    {
        return Mathf.Sin(phase * MathUtils.TwoPi);
    }
}
