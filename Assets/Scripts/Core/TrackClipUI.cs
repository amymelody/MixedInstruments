using Melanchall.DryWetMidi.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TrackClipUI : MonoBehaviour
{
    // reference track module
    // handle button interactions and visuals

    const string k_NoClipsText = "No clips";

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
    List<string> m_MidiFileNames = new List<string>();

    public void AssignTrack(TrackModule trackModule)
    {
        m_TrackModule = trackModule;

        MidiFilesManager.GetMidiFiles(m_MidiFiles, m_MidiFileNames, m_TrackModule.instrument.GetType());
        m_ClipText.text = m_MidiFileNames.Count > 0 ? m_MidiFileNames[m_ClipIndex] : k_NoClipsText;

        m_RecordButton.onTouchEnd.AddListener(OnRecordButtonTouched);
        m_PlayButton.onTouchEnd.AddListener(OnPlayButtonTouched);
        m_CycleClipButton.onTouchEnd.AddListener(OnCycleClipButtonTouched);
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
    }

    void OnPlayButtonTouched(TouchElement button)
    {
        var instrument = m_TrackModule.instrument;
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
}
