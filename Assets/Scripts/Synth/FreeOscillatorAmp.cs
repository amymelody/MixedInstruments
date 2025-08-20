using Melanchall.DryWetMidi.MusicTheory;
using Unity.Mathematics;
using UnityEngine;

public class FreeOscillatorAmp : Amp
{
    [SerializeField]
    SerializedNote m_LowestNote = new() { NoteName = NoteName.C, Octave = 0 };

    [SerializeField]
    SerializedNote m_HighestNote = new() { NoteName = NoteName.C, Octave = 6 };


    Vector2 m_FrequencyLogRange;
    float m_Frequency;

    public float frequency
    {
        get => m_Frequency;
        set => m_Frequency = math.max(value, 0f);
    }

    Oscillator m_Osc = new();

    float m_Phase;
    double m_ExpectedTime;

    void OnEnable()
    {
        var lowFreq = NoteUtils.FundamentalFrequencies[NoteUtilities.GetNoteNumber(m_LowestNote.NoteName, m_LowestNote.Octave)];
        var highFreq = NoteUtils.FundamentalFrequencies[NoteUtilities.GetNoteNumber(m_HighestNote.NoteName, m_HighestNote.Octave)];
        m_FrequencyLogRange = new Vector2(math.log2(lowFreq), math.log2(highFreq));
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (channels != 2) // currently only support for 2 channels
            return;

        // sync up with DSP time
        var timeDiff = AudioSettings.dspTime - m_ExpectedTime;
        m_Phase += (float)timeDiff * frequency;
        if (m_Phase < 0) m_Phase = 1f - m_Phase;
        m_ExpectedTime = AudioSettings.dspTime + ((double)data.Length / channels) / sampleRate;

        // keeping track of persistent phase handles changes to frequency
        var timeStep = 1f / sampleRate;
        var phaseStep = timeStep * frequency;
        for (var i = 0; i < data.Length; i = i + 2)
        {
            m_Phase += phaseStep;
            var sample = m_Osc.Sample(m_Phase) * volume;
            data[i] += sample;
            data[i + 1] = data[i];
        }

        m_Phase = m_Phase % 1f;
    }

    public void LerpFrequency(float value)
    {
        var freqLog = math.lerp(m_FrequencyLogRange.x, m_FrequencyLogRange.y, value);
        frequency = math.exp2(freqLog);
    }
}
