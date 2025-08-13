using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MIDIInstrument : MonoBehaviour
{
    AudioSource m_AudioSource;

    int m_SampleRate;

    void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
        m_SampleRate = AudioSettings.outputSampleRate;
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (channels > 2) // don't know what to do with more than 2 channels
            return;

        var frequency = 261.63f; // C4

        var firstPhase = (float)(AudioSettings.dspTime % (1.0d / (double)frequency));
        var currentDataStep = 0;
        for (var i = 0; i < data.Length; i++)
        {
            var phase = (firstPhase + (float)currentDataStep / m_SampleRate) * frequency;
            var sample = Sample(phase);
            data[i] += sample;
            currentDataStep++;

            if (channels == 2)
            {
                data[i + 1] = data[i];
                i++;
            }
        }
    }

    float Sample(float phase)
    {
        return Mathf.Sin(phase * MathUtils.TwoPi);
    }
}
