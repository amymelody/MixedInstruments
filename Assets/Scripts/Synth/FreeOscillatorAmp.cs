using UnityEngine;

public class FreeOscillatorAmp : Amp
{
    public float frequency { get; set; }

    Oscillator m_Osc = new Oscillator();

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (channels != 2) // currently only support for 2 channels
            return;

        for (var i = 0; i < data.Length; i = i + 2)
        {
            var phase = ((float)(AudioSettings.dspTime % (1.0d / (double)frequency))) * frequency;
            var sample = m_Osc.Sample(phase) * volume;
            data[i] += sample;
            data[i + 1] = data[i];
        }
    }
}
