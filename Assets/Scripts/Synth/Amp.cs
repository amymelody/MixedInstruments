using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public abstract class Amp : MonoBehaviour
{
    public float volume = 0.5f;

    public int sampleRate { get; private set; }

    protected virtual void Awake()
    {
        sampleRate = AudioSettings.outputSampleRate;
    }
}
