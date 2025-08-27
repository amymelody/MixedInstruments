using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public abstract class Amp : MonoBehaviour
{
    public float volume = 0.5f;

    public int sampleRate { get; private set; }

    public float sampleTimeStep { get; private set; }

    AudioSource m_AudioSource;
    public AudioSource audioSource
    {
        get
        {
            if (m_AudioSource == null) m_AudioSource = GetComponent<AudioSource>();
            return m_AudioSource;
        }
    }

    protected virtual void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
        sampleRate = AudioSettings.outputSampleRate;
        sampleTimeStep = 1f / sampleRate;
    }
}
