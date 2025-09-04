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

    void OnEnable()
    {
        var lowFreq = NoteUtils.FundamentalFrequencies[NoteUtilities.GetNoteNumber(m_LowestNote.NoteName, m_LowestNote.Octave)];
        var highFreq = NoteUtils.FundamentalFrequencies[NoteUtilities.GetNoteNumber(m_HighestNote.NoteName, m_HighestNote.Octave)];
        m_FrequencyLogRange = new Vector2(math.log2(lowFreq), math.log2(highFreq));
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        OnBeforeReadSamples();

        // keeping track of persistent phase handles changes to frequency
        for (var i = 0; i < data.Length; i = i + channels)
        {
            OnBeforeSample();
            m_Phase += sampleTimeStep * frequency;
            var sample = m_Osc.Sample(m_Phase) * volume;
            data[i] += sample;
            for (var j = 1; j < channels; j++)
            {
                data[i + j] = data[i];
            }
        }

        m_Phase = m_Phase % 1f;
    }

    protected virtual void OnBeforeReadSamples() { }

    protected virtual void OnBeforeSample() { }

    public void LerpFrequency(float value)
    {
        var freqLog = math.lerp(m_FrequencyLogRange.x, m_FrequencyLogRange.y, value);
        frequency = math.exp2(freqLog);
    }
}
