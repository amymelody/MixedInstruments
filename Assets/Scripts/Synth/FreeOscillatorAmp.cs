using UnityEngine;

public class FreeOscillatorAmp : Amp
{
    [SerializeField]
    float m_Frequency;

    public float frequency
    {
        get
        {
            if (m_Frequency < 0f) m_Frequency = 0f;
            return m_Frequency;
        }
        set
        {
            m_Frequency = Mathf.Max(value, 0f);
        }
    }

    Oscillator m_Osc = new();

    float m_Phase;
    double m_ExpectedTime;

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
    }
}
