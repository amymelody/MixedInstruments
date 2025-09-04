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
        m_LastBar = TimingUtils.metronome.bar;
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
            var bar = TimingUtils.metronome.bar;
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
        m_LastEventTick = TimingUtils.metronome.tickAtStartOfBar;
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
            RecordNoteEvent<NoteOnEvent>(noteNumber, SevenBitNumber.MaxValue);
        }
    }

    public virtual void NoteOff(SevenBitNumber noteNumber)
    {
        amp.NoteOff(noteNumber);

        if (recordingState == RecordingState.Active)
        {
            RecordNoteEvent<NoteOffEvent>(noteNumber, SevenBitNumber.MaxValue);
        }
    }

    void RecordNoteEvent<TNoteEvent>(SevenBitNumber noteNumber, SevenBitNumber velocity) where TNoteEvent : NoteEvent, new()
    {
        var noteEvent = new TNoteEvent();
        noteEvent.NoteNumber = noteNumber;
        noteEvent.Velocity = velocity;

        var tick = TimingUtils.metronome.tick;
        switch (TimingSettings.recordQuantization)
        {
            case Quantization.Quarter:
                tick = QuantizedTick(tick, 1);
                break;
            case Quantization.Eighth:
                tick = QuantizedTick(tick, 2);
                break;
            case Quantization.EighthTriplets:
                tick = QuantizedTick(tick, 3);
                break;
            case Quantization.EighthAndTriplets:
                var eighth = QuantizedTick(tick, 2);
                var eighthTriplet = QuantizedTick(tick, 3);
                if (Mathf.Abs(tick - eighth) < Mathf.Abs(tick - eighthTriplet))
                    tick = eighth;
                else
                    tick = eighthTriplet;
                break;
            case Quantization.Sixteenth:
                tick = QuantizedTick(tick, 4);
                break;
            case Quantization.SixteenthTriplets:
                tick = QuantizedTick(tick, 6);
                break;
            case Quantization.SixteenthAndTriplets:
                var sixteenth = QuantizedTick(tick, 4);
                var sixteenthTriplet = QuantizedTick(tick, 6);
                if (Mathf.Abs(tick - sixteenth) < Mathf.Abs(tick - sixteenthTriplet))
                    tick = sixteenth;
                else
                    tick = sixteenthTriplet;
                break;
            case Quantization.ThirtySecond:
                tick = QuantizedTick(tick, 8);
                break;
            default:
                break;
        }

        noteEvent.DeltaTime = tick - m_LastEventTick;
        m_LastEventTick = tick;

        recordingEvents.Add(noteEvent);
    }

    long QuantizedTick(long tick, short quantPerQuarter)
    {
        var ticksPerQuant = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote / quantPerQuarter;
        var offTicks = tick % ticksPerQuant;
        if (offTicks < ticksPerQuant / 2)
            return tick - offTicks; // round down
        
        return tick + ticksPerQuant - offTicks; // round up
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
