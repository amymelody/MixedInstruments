using Melanchall.DryWetMidi.MusicTheory;
using System;
using UnityEngine;

[RequireComponent(typeof(TouchElement))]
public class TouchNote : MonoBehaviour
{
    public Note Note;

    public Action<Note> onNoteTouchOn;
    public Action<Note> onNoteTouchOff;

    TouchElement m_TouchElement;

    void Start()
    {
        m_TouchElement = GetComponent<TouchElement>();
        m_TouchElement.onFirstTouchStarting.AddListener(OnFirstTouchStarting);
        m_TouchElement.onLastTouchEnded.AddListener(OnLastTouchEnded);
    }

    void OnDestroy()
    {
        m_TouchElement.onFirstTouchStarting.RemoveListener(OnFirstTouchStarting);
        m_TouchElement.onLastTouchEnded.RemoveListener(OnLastTouchEnded);
    }

    void OnFirstTouchStarting()
    {
        onNoteTouchOn?.Invoke(Note);
    }

    void OnLastTouchEnded()
    {
        onNoteTouchOff?.Invoke(Note);
    }
}
