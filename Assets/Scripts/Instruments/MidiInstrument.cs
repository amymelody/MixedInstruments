using Melanchall.DryWetMidi.Core;
using System.Collections.Generic;
using UnityEngine;

public enum RecordingState
{
    Inactive,
    Primed,
    Active
}

public abstract class MidiInstrument : MonoBehaviour
{
    public RecordingState recordingState { get; private set; }

    public List<MidiEvent> recordingEvents { get; private set; }

    float m_LastBarPhase;

    public void PrimeForRecording()
    {
        recordingState = RecordingState.Primed;
        m_LastBarPhase = TimingUtils.GetBarPhase();
        recordingEvents = new List<MidiEvent>();
    }

    public void StopRecording()
    {
        if (recordingState == RecordingState.Primed)
        {
            StopLeadIn();
        }
        else if (recordingState == RecordingState.Active)
        {

        }

        recordingState = RecordingState.Inactive;
    }

    protected virtual void Update()
    {
        if (recordingState == RecordingState.Primed)
        {
            var barPhase = TimingUtils.GetBarPhase();
            if (barPhase < m_LastBarPhase) // we've looped around to next bar
            {
                if (TimingSettings.recordLeadIn && !TimingSettings.recordLeadInActive)
                {
                    TimingSettings.recordLeadInActive = true;
                }
                else
                {
                    StartRecording();
                }
            }

            m_LastBarPhase = barPhase;
        }
    }

    void StartRecording()
    {
        StopLeadIn();
        recordingState = RecordingState.Active;
    }

    void StopLeadIn()
    {
        TimingSettings.recordLeadInActive = false;
    }
}
