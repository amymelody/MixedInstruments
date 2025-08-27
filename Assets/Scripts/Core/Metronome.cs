using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Metronome : Amp
{
    public int signatureHi = 4;
    public int signatureLo = 4;

    private double nextTick = 0.0F;
    private float amp = 0.0F;
    private float phase = 0.0F;
    private int accent;

    protected override void Awake()
    {
        base.Awake();
        accent = signatureHi;
        double startTick = AudioSettings.dspTime;
        nextTick = startTick * sampleRate;
    }

    // based on Unity's example code https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnAudioFilterRead.html
    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!TimingSettings.playMetronome)
            return;

        double samplesPerTick = sampleRate * 60.0F / TimingSettings.bpm * 4.0F / signatureLo;
        double sample = AudioSettings.dspTime * sampleRate;
        int dataLen = data.Length / channels;

        int n = 0;
        while (n < dataLen)
        {
            float x = volume * amp * Mathf.Sin(phase);
            int i = 0;
            while (i < channels)
            {
                data[n * channels + i] += x;
                i++;
            }
            while (sample + n >= nextTick)
            {
                nextTick += samplesPerTick;
                amp = 1.0F;
                if (++accent > signatureHi)
                {
                    accent = 1;
                    amp *= 2.0F;
                }
            }
            phase += amp * 0.3F;
            phase %= MathUtils.TwoPi;
            amp *= 0.993F;
            n++;
        }
    }
}
