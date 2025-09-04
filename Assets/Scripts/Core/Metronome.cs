using Melanchall.DryWetMidi.Core;
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

    public long bar { get; private set; }

    public long tick { get; private set; }

    int m_TimeSignatureNumerator;
    int m_TimeSignatureDenominator;
    float m_BarPhaseStep;
    float m_BarDuration;
    float m_BeatDuration;
    float m_BeatBarFraction;
    float m_QuarterPerBar;
    long m_TickAtStartOfBar;

    protected override void Awake()
    {
        base.Awake();

        m_TimeSignatureNumerator = TimingSettings.timeSignature.numerator;
        m_TimeSignatureDenominator = TimingSettings.timeSignature.denominator;
        UpdateTiming();
        bar = (long)(AudioSettings.dspTime / m_BarDuration);
        barPhase = (float)(AudioSettings.dspTime % m_BarDuration) / m_BarDuration;
        m_TickAtStartOfBar = (long)((float)(m_QuarterPerBar * bar) * TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote);
        tick = m_TickAtStartOfBar + (long)((float)(m_QuarterPerBar * barPhase) * TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote);
    }

    void UpdateTiming()
    {
        var bpm = TimingSettings.bpm;
        var beatsPerBar = m_TimeSignatureNumerator;
        var quarterDuration = 60f / bpm;
        var beatToQuarterRatio = 4f / m_TimeSignatureDenominator;
        m_QuarterPerBar = beatToQuarterRatio * beatsPerBar;
        m_BeatDuration = beatToQuarterRatio * quarterDuration;
        m_BarDuration = m_BeatDuration * beatsPerBar;
        m_BeatBarFraction = 1f / beatsPerBar;
        m_BarPhaseStep = sampleTimeStep / m_BarDuration;
    }

    protected override void OnBeforeReadSamples()
    {
        base.OnBeforeReadSamples();
        UpdateTiming();
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

        barPhase += m_BarPhaseStep;
        tick = m_TickAtStartOfBar + (long)((float)(m_QuarterPerBar * barPhase) * TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote);
        if (barPhase > 1f) // next bar
        {
            NextBar();
        }
    }

    void NextBar()
    {
        barPhase %= 1f;
        bar++;
        Debug.Log(tick - m_TickAtStartOfBar);
        m_TickAtStartOfBar = tick;
        // only update time signature when bar changes, to simplify tick counting
        m_TimeSignatureNumerator = TimingSettings.timeSignature.numerator;
        m_TimeSignatureDenominator = TimingSettings.timeSignature.denominator;
    }
}
