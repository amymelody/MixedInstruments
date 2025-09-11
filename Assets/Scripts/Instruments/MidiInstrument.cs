using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum RecordingState
{
    Inactive,
    Primed,
    Active
}

public abstract class MidiInstrument : MonoBehaviour
{
    public abstract MidiAmp amp { get; }

    public RecordingState recordingState { get; private set; }

    public List<MidiEvent> recordingEvents { get; private set; } = new List<MidiEvent>();

    public List<MidiEvent> playbackEvents { get; private set; } = new List<MidiEvent>();

    public bool isPlaying { get; private set; }

    public event Action onRecordingStart;
    public event Action onRecordingComplete;

    long m_LastBar;

    long m_LastRecordedEventTick;

    long m_PlaybackLengthInTicks;
    int m_NextPlaybackEventIndex;
    long m_LastPlayedEventTick;
    long m_OverdubStartTick;

    public void PrimeForRecording()
    {
        recordingState = RecordingState.Primed;
        m_LastBar = Metronome.instance.bar;
        recordingEvents.Clear();

        // if overdub, start immediately
        if (isPlaying)
            StartRecording(m_OverdubStartTick);
    }

    public void StopRecording()
    {
        if (recordingState == RecordingState.Primed)
        {
            StopLeadIn();
        }
        else if (recordingState == RecordingState.Active)
        {
            if (isPlaying)
            {
                MergeOverdub();
            }
            else
            {
                // Add dummy note event marking endpoint (end of current bar)
                // TODO: different event type?
                var endTick = Metronome.instance.tick + Metronome.instance.ticksLeftInBar;
                var endEvent = new NoteOnEvent();
                endEvent.DeltaTime = endTick - m_LastRecordedEventTick;
                recordingEvents.Add(endEvent);
            }

            onRecordingComplete?.Invoke();
        }

        recordingState = RecordingState.Inactive;
    }

    public void StartPlayback(MidiFile midiFile)
    {
        isPlaying = true;

        playbackEvents.Clear();
        foreach (var trackChunk in midiFile.GetTrackChunks())
        {
            playbackEvents.AddRange(trackChunk.Events);
        }

        // start playback from a place that will line up the start of the next bar with the start of the recording
        // wherever we are in the current bar, start playback from that part of the last bar in the recording
        m_PlaybackLengthInTicks = playbackEvents.DefaultIfEmpty().Sum((MidiEvent e) => e?.DeltaTime ?? 0);
        var ticksLeftInBar = Metronome.instance.ticksLeftInBar;
        long lastEventTick = 0;
        for (var i = 0; i < playbackEvents.Count; i++)
        {
            var midiEvent = playbackEvents[i];
            var eventTick = lastEventTick + midiEvent.DeltaTime;
            var ticksLeftUntilLoop = m_PlaybackLengthInTicks - eventTick;
            // checking strictly less than only works if ticksLeftInBar > 0, which we ensure is always the case as long as ticksPerBar > 0
            if (ticksLeftUntilLoop < ticksLeftInBar)
            {
                // we've found the NEXT event that should play
                m_NextPlaybackEventIndex = i;
                var ticksSinceLastEvent = midiEvent.DeltaTime - (ticksLeftInBar - ticksLeftUntilLoop);
                m_LastPlayedEventTick = Metronome.instance.tick - ticksSinceLastEvent;
                m_OverdubStartTick = m_LastPlayedEventTick - lastEventTick;
                break;
            }

            lastEventTick = eventTick;
        }
    }

    public void StopPlayback()
    {
        // save overdub if we stop playing
        if (recordingState == RecordingState.Active)
            StopRecording();

        isPlaying = false;
    }

    protected virtual void Update()
    {
        var bar = Metronome.instance.bar;

        if (recordingState == RecordingState.Primed)
        {
            if (bar > m_LastBar) // we've ticked to next bar
            {
                if (TimingSettings.recordLeadIn && !TimingSettings.recordLeadInActive)
                {
                    // TODO: time this in metronome itself so we hear accent tick at beginning instead of end
                    TimingSettings.recordLeadInActive = true;
                }
                else
                {
                    StartRecording(Metronome.instance.tickAtStartOfBar);
                }
            }
        }

        if (isPlaying)
        {
            // process all events that fall within time since last event
            var deltaTicks = Metronome.instance.tick - m_LastPlayedEventTick;
            while (playbackEvents[m_NextPlaybackEventIndex].DeltaTime <= deltaTicks)
            {
                var midiEvent = playbackEvents[m_NextPlaybackEventIndex];
                deltaTicks -= midiEvent.DeltaTime;
                m_LastPlayedEventTick += midiEvent.DeltaTime;

                // count last event as "played" but skip processing - it just marks end of track
                if (m_NextPlaybackEventIndex == playbackEvents.Count - 1)
                {
                    m_NextPlaybackEventIndex = 0;

                    // if overdub, merge at end of playback loop and start recording at next tick
                    if (recordingState == RecordingState.Active)
                    {
                        MergeOverdub();
                        recordingEvents.Clear();
                        m_LastRecordedEventTick = m_LastPlayedEventTick;
                    }
                }
                else
                {
                    ProcessEvent(midiEvent);
                    m_NextPlaybackEventIndex++;
                }
            }
        }

        m_LastBar = bar;
    }

    void ProcessEvent(MidiEvent midiEvent)
    {
        switch (midiEvent.EventType)
        {
            case MidiEventType.NoteOn:
                var noteOnEvent = midiEvent as NoteOnEvent;
                NoteOn(noteOnEvent.NoteNumber, true);
                break;
            case MidiEventType.NoteOff:
                var noteOffEvent = midiEvent as NoteOffEvent;
                NoteOff(noteOffEvent.NoteNumber, true);
                break;
            default:
                break;
        }
    }

    void StartRecording(long startTick)
    {
        StopLeadIn();
        recordingState = RecordingState.Active;
        m_LastRecordedEventTick = startTick;
        onRecordingStart?.Invoke();
    }

    void StopLeadIn()
    {
        TimingSettings.recordLeadInActive = false;
    }

    public virtual void NoteOn(SevenBitNumber noteNumber, bool isPlayback = false)
    {
        amp.NoteOn(noteNumber);

        if (recordingState == RecordingState.Active && !isPlayback)
        {
            RecordNoteEvent<NoteOnEvent>(noteNumber, SevenBitNumber.MaxValue);
        }
    }

    public virtual void NoteOff(SevenBitNumber noteNumber, bool isPlayback = false)
    {
        amp.NoteOff(noteNumber);

        if (recordingState == RecordingState.Active && !isPlayback)
        {
            RecordNoteEvent<NoteOffEvent>(noteNumber, SevenBitNumber.MaxValue);
        }
    }

    void RecordNoteEvent<TNoteEvent>(SevenBitNumber noteNumber, SevenBitNumber velocity) where TNoteEvent : NoteEvent, new()
    {
        var noteEvent = new TNoteEvent();
        noteEvent.NoteNumber = noteNumber;
        noteEvent.Velocity = velocity;

        var tick = Metronome.instance.tick;
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

        noteEvent.DeltaTime = tick - m_LastRecordedEventTick;
        m_LastRecordedEventTick = tick;

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

    void MergeOverdub()
    {
        if (recordingEvents.Count == 0)
            return;

        var mergedEvents = new List<MidiEvent>();
        var playI = 0;
        var recI = 0;
        long lastTick = 0;
        long lastPlayTick = 0;
        long lastRecTick = 0;
        while (playI < playbackEvents.Count && recI < recordingEvents.Count)
        {
            var playEvent = playbackEvents[playI];
            var recEvent = recordingEvents[recI];
            var playTick = lastPlayTick + playEvent.DeltaTime;
            var recTick = lastRecTick + recEvent.DeltaTime;
            if (playTick <= recTick)
            {
                playEvent.DeltaTime = playTick - lastTick;
                mergedEvents.Add(playEvent);
                lastPlayTick = playTick;
                lastTick = playTick;
                playI++;
            }
            else
            {
                recEvent.DeltaTime = recTick - lastTick;
                mergedEvents.Add(recEvent);
                lastRecTick = recTick;
                lastTick = recTick;
                recI++;
            }
        }

        while (playI < playbackEvents.Count)
        {
            var playEvent = playbackEvents[playI];
            var playTick = lastPlayTick + playEvent.DeltaTime;
            playEvent.DeltaTime = playTick - lastTick;
            mergedEvents.Add(playEvent);
            lastPlayTick = playTick;
            lastTick = playTick;
            playI++;
        }

        while (recI < recordingEvents.Count)
        {
            var recEvent = recordingEvents[recI];
            var recTick = lastRecTick + recEvent.DeltaTime;
            recEvent.DeltaTime = recTick - lastTick;
            mergedEvents.Add(recEvent);
            lastRecTick = recTick;
            lastTick = recTick;
            recI++;
        }

        playbackEvents.Clear();
        playbackEvents.AddRange(mergedEvents);
    }
}
