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
        m_TouchElement.onTouchStart.AddListener(OnTouchStart);
        m_TouchElement.onTouchEnd.AddListener(OnTouchEnd);
    }

    void OnDestroy()
    {
        m_TouchElement.onTouchStart.RemoveListener(OnTouchStart);
        m_TouchElement.onTouchEnd.RemoveListener(OnTouchEnd);
    }

    void OnTouchStart(TouchElement touchElement)
    {
        onNoteTouchOn?.Invoke(Note);
    }

    void OnTouchEnd(TouchElement touchElement)
    {
        onNoteTouchOff?.Invoke(Note);
    }
}
