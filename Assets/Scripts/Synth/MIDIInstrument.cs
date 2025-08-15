using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MIDIInstrument : MonoBehaviour
{
    public float volume = 0.5f;

    int m_SampleRate;

    MidiNote[] m_Notes;
    IMidiSynth m_Synth;

    void Awake()
    {
        m_SampleRate = AudioSettings.outputSampleRate;

        m_Notes = new MidiNote[SevenBitNumber.MaxValue];
        for (var i = 0; i < m_Notes.Length; i++)
        {
            m_Notes[i].note = Note.Get((SevenBitNumber)i);
        }

        m_Synth = new SubtractiveSynth();
    }

    public void NoteOn(Note note)
    {
        m_Notes[note.NoteNumber].isActive = true;
        m_Notes[note.NoteNumber].onTime = AudioSettings.dspTime;
    }

    public void NoteOff(Note note)
    {
        m_Notes[note.NoteNumber].isActive = false;
        m_Notes[note.NoteNumber].offTime = AudioSettings.dspTime;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (channels != 2) // currently only support for 2 channels
            return;

        for (var i = 0; i < m_Notes.Length; i++)
        {
            var note = m_Notes[i];
            if (!note.isActive)
                continue;

            var firstSampleNoteTime = AudioSettings.dspTime - note.onTime;
            var currentDataStep = 0;
            for (var j = 0; j < data.Length; j = j + 2)
            {
                var sample = m_Synth.Sample(note, firstSampleNoteTime + (double)currentDataStep / m_SampleRate) * volume;
                data[j] += sample;
                data[j + 1] = sample;
                currentDataStep++;
            }
        }
    }
}
