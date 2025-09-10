using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
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

    long m_LastBar;

    long m_LastRecordedEventTick;

    long m_PlaybackLengthInTicks;
    int m_NextPlaybackEventIndex;
    long m_LastPlayedEventTick;

    public void PrimeForRecording()
    {
        recordingState = RecordingState.Primed;
        m_LastBar = Metronome.instance.bar;
        recordingEvents.Clear();
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
                break;
            }

            lastEventTick = eventTick;
        }
    }

    public void StopPlayback()
    {
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
                    TimingSettings.recordLeadInActive = true;
                }
                else
                {
                    StartRecording();
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
                ProcessEvent(midiEvent);
                deltaTicks -= midiEvent.DeltaTime;
                m_NextPlaybackEventIndex = (m_NextPlaybackEventIndex + 1) % playbackEvents.Count;
            }
        }

        m_LastBar = bar;
    }

    void ProcessEvent(MidiEvent midiEvent)
    {

    }

    void StartRecording()
    {
        StopLeadIn();
        recordingState = RecordingState.Active;
        m_LastRecordedEventTick = Metronome.instance.tickAtStartOfBar;
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

    void SaveRecording()
    {
        // Add dummy note event marking endpoint (end of current bar)
        // TODO: different event type?
        var endTick = Metronome.instance.tick + Metronome.instance.ticksLeftInBar;
        var endEvent = new NoteOnEvent();
        endEvent.DeltaTime = endTick - m_LastRecordedEventTick;
        recordingEvents.Add(endEvent);

        // TODO: handle overdub
        var trackChunk = new TrackChunk(recordingEvents);
        var midiFile = new MidiFile(trackChunk);

        // TODO: record time signature and bpm changes
        midiFile.ReplaceTempoMap(TempoMap.Create(Tempo.FromBeatsPerMinute(TimingSettings.bpm), TimingSettings.timeSignature));

        MidiFilesManager.WriteNewMidiFile(midiFile, GetType());
        StartPlayback(midiFile);
    }
}
