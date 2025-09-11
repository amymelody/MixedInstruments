using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class TrackClipUI : MonoBehaviour
{
    const string k_NoClipsText = "No clips";
    const string k_RecordText = "Record";
    const string k_RecordingText = "Recording";
    const string k_PlayText = "Play";

    static readonly Color k_RecordColorOff = Color.white;
    static readonly Color k_RecordColorOn = Color.red;

    [SerializeField]
    TouchElement m_RecordButton;

    [SerializeField]
    TouchElement m_PlayButton;

    [SerializeField]
    TouchElement m_CycleClipButton;

    [SerializeField]
    TextMeshProUGUI m_ClipText;

    TrackModule m_TrackModule;

    int m_ClipIndex;
    List<MidiFile> m_MidiFiles = new List<MidiFile>();
    List<string> m_MidiFilePaths = new List<string>();

    public void AssignTrack(TrackModule trackModule)
    {
        m_TrackModule = trackModule;

        MidiFilesManager.GetMidiFiles(m_MidiFiles, m_MidiFilePaths, m_TrackModule.instrument.GetType());
        m_ClipText.text = m_MidiFilePaths.Count > 0 ? Path.GetFileNameWithoutExtension(m_MidiFilePaths[m_ClipIndex]) : k_NoClipsText;

        m_RecordButton.buttonRenderer.material.color = k_RecordColorOff;
        m_RecordButton.text.text = k_RecordText;
        m_RecordButton.onTouchEnd.AddListener(OnRecordButtonTouched);
        m_PlayButton.onTouchEnd.AddListener(OnPlayButtonTouched);
        m_CycleClipButton.onTouchEnd.AddListener(OnCycleClipButtonTouched);

        m_TrackModule.instrument.onRecordingStart += OnRecordingStart;
        m_TrackModule.instrument.onRecordingComplete += OnRecordingComplete;
    }

    void OnRecordButtonTouched(TouchElement button)
    {
        var instrument = m_TrackModule.instrument;
        if (instrument.recordingState == RecordingState.Inactive)
        {
            instrument.PrimeForRecording();
            return;
        }

        instrument.StopRecording();
        m_RecordButton.buttonRenderer.material.color = k_RecordColorOff;
        m_RecordButton.text.text = k_RecordText;
    }

    void OnPlayButtonTouched(TouchElement button)
    {
        var instrument = m_TrackModule.instrument;
        if (instrument.recordingState != RecordingState.Inactive) // playing after recording has started can cause overdub issues
            return;

        if (instrument.isPlaying)
        {
            instrument.StopPlayback();
        }
        else if (m_MidiFiles.Count > 0)
        {
            instrument.StartPlayback(m_MidiFiles[m_ClipIndex]);
        }
    }

    void OnCycleClipButtonTouched(TouchElement button)
    {

    }

    void OnRecordingStart()
    {
        m_RecordButton.buttonRenderer.material.color = k_RecordColorOn;
        m_RecordButton.text.text = k_RecordingText;
    }

    void OnRecordingComplete()
    {
        var instrument = m_TrackModule.instrument;
        if (instrument.isPlaying) // save overdub, overwrite file
        {
            var trackChunk = new TrackChunk(instrument.playbackEvents);
            var midiFile = new MidiFile(trackChunk);
            MidiFilesManager.OverwriteMidiFile(midiFile, m_MidiFilePaths[m_ClipIndex]);
        }
        else
        {
            var trackChunk = new TrackChunk(instrument.recordingEvents);
            var midiFile = new MidiFile(trackChunk);

            // TODO: record time signature and bpm changes
            midiFile.ReplaceTempoMap(TempoMap.Create(Tempo.FromBeatsPerMinute(TimingSettings.bpm), TimingSettings.timeSignature));

            MidiFilesManager.WriteNewMidiFile(midiFile, instrument.GetType());
            instrument.StartPlayback(midiFile);
        }
    }

    void Update()
    {
        var instrument = m_TrackModule.instrument;
        var metronome = Metronome.instance;
        if (instrument.recordingState == RecordingState.Primed)
        {
            var colorLerp = Mathf.Abs(metronome.beatPhase - 0.5f) * 2f;
            m_RecordButton.buttonRenderer.material.color = Color.Lerp(k_RecordColorOff, k_RecordColorOn, colorLerp);

            var beatsLeft = 1 + metronome.beatsPerBar - metronome.beatInBar;
            if (TimingSettings.recordLeadIn && !TimingSettings.recordLeadInActive)
                beatsLeft += metronome.beatsPerBar;

            m_RecordButton.text.text = beatsLeft.ToString();
        }
    }
}
