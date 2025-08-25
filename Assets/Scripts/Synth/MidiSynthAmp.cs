using UnityEngine;

public class MidiSynthAmp : MidiAmp
{
    IMidiSynth m_Synth;

    protected override void Awake()
    {
        base.Awake();

        m_Synth = new SubtractiveSynth();
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
                var sample = m_Synth.Sample(note, firstSampleNoteTime + (double)currentDataStep / sampleRate) * volume;
                data[j] += sample;
                data[j + 1] = data[j];
                currentDataStep++;
            }
        }
    }
}
