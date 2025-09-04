using UnityEngine;

public class Metronome : FreeOscillatorAmp
{
    [SerializeField]
    float m_AccentTickFrequency = 1500f;

    [SerializeField]
    float m_RegularTickFrequency = 1000f;

    [SerializeField]
    float m_TickDecay = 0.015f;

    public float barPhase { get; private set; }

    float m_BarPhaseStep;
    float m_BarDuration;
    float m_BeatDuration;
    float m_BeatBarFraction;

    protected override void Awake()
    {
        base.Awake();

        var bpm = TimingSettings.bpm;
        var timeSignature = TimingSettings.timeSignature;
        var beatsPerBar = timeSignature.numerator;
        var beatToQuarterRatio = 4f / timeSignature.denominator;
        m_BeatDuration = beatToQuarterRatio * 60f / bpm;
        m_BarDuration = m_BeatDuration * beatsPerBar;
        barPhase = (float)(AudioSettings.dspTime % m_BarDuration) / m_BarDuration;
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
        barPhase += barPhaseDiff;
        if (barPhase < 0)
            barPhase = 1f - barPhase;
        else if (barPhase > 1f)
            barPhase %= 1f;

        m_BarPhaseStep = sampleTimeStep / m_BarDuration;
    }

    protected override void OnBeforeSample()
    {
        base.OnBeforeSample();

        if (TimingSettings.playMetronome || TimingSettings.recordLeadInActive)
        {
            var accent = barPhase < m_BeatBarFraction;
            var tickInitFrequency = accent ? m_AccentTickFrequency : m_RegularTickFrequency;
            var tickInitVolume = accent ? 1f : 0.5f;
            var beatPhase = (barPhase % m_BeatBarFraction) / m_BeatBarFraction;
            var beatTime = m_BeatDuration * beatPhase;
            var tickStrength = 1f - Mathf.InverseLerp(0f, m_TickDecay, beatTime);
            frequency = Mathf.Lerp(0f, tickInitFrequency, tickStrength);
            volume = Mathf.Lerp(0f, tickInitVolume, tickStrength);
        }
        else
        {
            volume = 0f;
        }

        barPhase = (barPhase + m_BarPhaseStep) % 1f;
    }
}
