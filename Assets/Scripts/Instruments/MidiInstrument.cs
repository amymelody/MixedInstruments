using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
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
    const string k_ClipNameFormat = "{0}_{1}";
    const string k_ClipDateTimeFormat = "yyyy_MM_dd_HHmmss";

    public abstract MidiAmp amp { get; }

    public RecordingState recordingState { get; private set; }

    public List<MidiEvent> recordingEvents { get; private set; }

    long m_LastBar;

    long m_LastEventTick;

    public void PrimeForRecording()
    {
        recordingState = RecordingState.Primed;
        m_LastBar = TimingUtils.GetBar();
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
            SaveRecording();
        }

        recordingState = RecordingState.Inactive;
    }

    protected virtual void Update()
    {
        if (recordingState == RecordingState.Primed)
        {
            var bar = TimingUtils.GetBar();
            if (bar > m_LastBar) // we've ticked to next bar
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

            m_LastBar = bar;
        }
    }

    void StartRecording()
    {
        StopLeadIn();
        recordingState = RecordingState.Active;
        m_LastEventTick = 0;
    }

    void StopLeadIn()
    {
        TimingSettings.recordLeadInActive = false;
    }

    public virtual void NoteOn(SevenBitNumber noteNumber)
    {
        amp.NoteOn(noteNumber);

        if (recordingState == RecordingState.Active)
        {
            var velocity = SevenBitNumber.MaxValue;
            var noteOnEvent = new NoteOnEvent(noteNumber, velocity);
        }
    }

    public virtual void NoteOff(SevenBitNumber noteNumber)
    {
        amp.NoteOff(noteNumber);

        if (recordingState == RecordingState.Active)
        {
            var velocity = SevenBitNumber.MaxValue;
            var noteOffEvent = new NoteOffEvent(noteNumber, velocity);
        }
    }

    void SaveRecording()
    {
        // TODO: handle overdub
        var trackChunk = new TrackChunk(recordingEvents);
        var midiFile = new MidiFile(trackChunk);
        var fileName = string.Format(k_ClipNameFormat, gameObject.name, DateTime.Now.ToString(k_ClipDateTimeFormat));
        midiFile.Write(fileName);
    }
}
