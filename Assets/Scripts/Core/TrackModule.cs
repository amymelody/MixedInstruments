using Melanchall.DryWetMidi.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TrackModule : Module
{
    [SerializeField]
    MidiInstrument m_Instrument;

    [SerializeField]
    Transform m_FrontUI;

    [SerializeField]
    float m_FrontUIPadding = 0.05f;

    [SerializeField]
    TouchElement m_RecordButton;

    [SerializeField]
    TouchElement m_PlayButton;

    [SerializeField]
    TouchElement m_CycleClipButton;

    [SerializeField]
    TextMeshProUGUI m_ClipText;

    int m_ClipIndex;
    List<MidiFile> m_MidiFiles = new List<MidiFile>();

    public override void AttachToTabletop(Tabletop tabletop)
    {
        m_FrontUI.localPosition = new Vector3(0f, 0f, -tabletop.depth * 0.5f + m_FrontUIPadding);
    }

    void OnEnable()
    {
        m_RecordButton.onTouchEnd.AddListener(OnRecordButtonTouched);
        m_PlayButton.onTouchEnd.AddListener(OnPlayButtonTouched);
        m_CycleClipButton.onTouchEnd.AddListener(OnCycleClipButtonTouched);
    }

    void OnDisable()
    {
        m_RecordButton.onTouchEnd.RemoveListener(OnRecordButtonTouched);
        m_PlayButton.onTouchEnd.RemoveListener(OnPlayButtonTouched);
        m_CycleClipButton.onTouchEnd.RemoveListener(OnCycleClipButtonTouched);
    }

    /*
     * MXI has an "always playing" model - the beat always advances along with DSP time
     * this means anything that is timed with the start of each bar (like recording or playing back a clip) needs to wait until that tick
     * 
     * recording:
     * - button pressed - tell MidiInstrument to prime for recording
     * - create a TrackChunk (https://melanchall.github.io/drywetmidi/api/Melanchall.DryWetMidi.Core.TrackChunk.html)
     * - while primed, wait for next bar to start (or +1 bar if lead-in is enabled)
     * - start recording mode
     *   - add note events to TrackChunk as they happen
     *     - note: if in playback mode, keep playing during recording to allow for overdub
     *   - quantize based on timing settings
     * - button pressed again - tell MidiInstrument to end recording
     * - fill out rest of current bar with empty space, and write to MIDI file (use existing file if overdub)
     *   - to create file, use MidiFile constructor passing the chunk (no need for header chunk), then call Write on the object
     * - automatically prime MidiInstrument for playback
     * 
     * playback:
     * - button pressed - tell MidiInstrument to prime for playback
     * - load midi file (MidiFile.Read("file.mid")) and get track chunk (assume there's only one)
     * - while primed, wait for next bar to start
     * - start playback mode
     * - TODO: figure out how to read from TrackChunk
     *   - likely need to read during Update, as OnAudioFilterRead is called on a different thread
     */

    void OnRecordButtonTouched(TouchElement button)
    {
        if (m_Instrument.recordingState == RecordingState.Inactive)
        {
            m_Instrument.PrimeForRecording();
            return;
        }

        m_Instrument.StopRecording();
    }

    void OnPlayButtonTouched(TouchElement button)
    {
        if (m_Instrument.isPlaying)
        {
            m_Instrument.StopPlayback();
        }
        else if (m_MidiFiles.Count > 0)
        {
            m_Instrument.StartPlayback(m_MidiFiles[m_ClipIndex]);
        }
    }

    void OnCycleClipButtonTouched(TouchElement button)
    {

    }
}
