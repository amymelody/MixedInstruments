using UnityEngine;

public class Metronome : FreeOscillatorAmp
{
    [SerializeField]
    float m_AccentTickFrequency = 1500f;

    [SerializeField]
    float m_RegularTickFrequency = 1000f;

    [SerializeField]
    float m_TickDecay = 0.015f;

    float m_BarPhase;
    float m_BarPhaseStep;
    float m_BarDuration;
    float m_BeatDuration;
    float m_BeatBarFraction;

    protected override void Awake()
    {
        base.Awake();

        m_BarPhase = 0f;
    }

    protected override void OnBeforeDSPTimeSync(double diffFromExpectedTime)
    {
        base.OnBeforeDSPTimeSync(diffFromExpectedTime);

        var bpm = TimingSettings.bpm;
        var timeSignature = TimingSettings.timeSignature;
        var beatsPerBar = timeSignature.numerator;
        var beatToQuarterRatio = 4f / timeSignature.denominator;
        m_BeatDuration = beatToQuarterRatio * 60f / bpm;
        m_BarDuration = m_BeatDuration * beatsPerBar;
        m_BeatBarFraction = 1f / beatsPerBar;
        var barPhaseDiff = (float)diffFromExpectedTime / m_BarDuration;
        m_BarPhase += barPhaseDiff;
        if (m_BarPhase < 0)
            m_BarPhase = 1f - m_BarPhase;
        else if (m_BarPhase > 1f)
            m_BarPhase %= 1f;

        m_BarPhaseStep = sampleTimeStep / m_BarDuration;
    }

    protected override void OnBeforeSample()
    {
        base.OnBeforeSample();

        if (TimingSettings.playMetronome)
        {
            var accent = m_BarPhase < m_BeatBarFraction;
            var tickInitFrequency = accent ? m_AccentTickFrequency : m_RegularTickFrequency;
            var tickInitVolume = accent ? 1f : 0.5f;
            var beatPhase = (m_BarPhase % m_BeatBarFraction) / m_BeatBarFraction;
            var beatTime = m_BeatDuration * beatPhase;
            var tickStrength = 1f - Mathf.InverseLerp(0f, m_TickDecay, beatTime);
            frequency = Mathf.Lerp(0f, tickInitFrequency, tickStrength);
            volume = Mathf.Lerp(0f, tickInitVolume, tickStrength);
        }
        else
        {
            volume = 0f;
        }

        m_BarPhase = (m_BarPhase + m_BarPhaseStep) % 1f;
    }
}
